using Contracts;
using Contracts.Parameters;
using FinePrint.Contracts.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MissionPartChecker
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    [WindowInitials(Caption = "Test Window", Visible = true)]
    public class MBWindow : MonoBehaviourWindow
    {
        internal override void Awake()
        {
            WindowCaption = "Test Window";
            WindowRect = new Rect(0, 0, 250, 50);
            DragEnabled = true;
            Visible = true;
        }

        internal override void Update()
        {
            //toggle whether its visible or not
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.F11))
                Visible = !Visible;
        }

        private U GetPrivateProperty<U, T>(T objectToRead, string propertyName) where T : class
        {
            var field = typeof(FacilitySystemsParameter)
                .GetField(propertyName, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            return (U)field.GetValue(objectToRead);
        }

        internal override void DrawWindow(int id)
        {

            var antennaRequired = false;
            var powerRequired = false;
            var dockingPortRequired = false;
            var wheelsRequred = false;
            var scienceRequired = false;
            var targetCapacity = 0;
            var labRequired = false;
            
            List<string> requiredParts = new List<string>();
            try
            {
                var contractSystem = ContractSystem.Instance;
                
                var contracts = contractSystem.Contracts.Where(c => c.ContractState == Contract.State.Active).ToList();

                foreach (var contract in contracts)
                {
                    var contractType = contract.GetType();
                    if (contract.GetParameter<ProbeSystemsParameter>() != null)
                    {
                        powerRequired = true;
                        antennaRequired = true;
                    }
                    if(contract.GetParameter<RecoverKerbal>() != null)
                    {
                        if (targetCapacity == 0) targetCapacity = 1;
                    }
                    if (contract.GetParameter<MobileBaseParameter>() != null)
                    {
                        wheelsRequred = true;
                    }
               
                    if (contract.GetParameter<FacilitySystemsParameter>() != null)
                    {
                        labRequired = true;
                        powerRequired = true;
                        antennaRequired = true;
                        dockingPortRequired = true;
                    }

                    var paramNode = new ConfigNode();
                    contract.Save(paramNode);
                    
                    var partValues = paramNode.GetValues("part");
                    if (partValues.Any())
                    {
                        foreach(var partVal in partValues)
                        {
                            requiredParts.Add(partVal);
                        }
                    }

                    partValues = paramNode.GetValues("targetCapacity");
                    if (partValues.Any())
                    {
                        foreach(var partVal in partValues)
                        {
                            var capacity = int.Parse(partVal);
                            if (targetCapacity < capacity)
                                targetCapacity = capacity;
                        }
                    }
                }               
            }
            catch(Exception ex)
            {
                LogFormatted("DEBUG: " + ex.Message);
            }

            foreach (var requiredPart in requiredParts)
            {
                var part = PartLoader.getPartInfoByName(requiredPart);
                var partPresent = EditorLogic.SortedShipList.Any(p => p.isAttached && p.name == requiredPart);
                RenderListItem(part.title, partPresent);
            }

    
            if (antennaRequired)
                RenderListItem("Antenna", Utils.shipHasModuleList(AntennaParts));
            if (powerRequired)
                RenderListItem("Power Supply", Utils.shipHasModuleList(PowerParts));
            if (dockingPortRequired)
                RenderListItem("Docking Port", Utils.shipHasModuleList(DockingParts));
            if (labRequired)
                RenderListItem("Science Lab", Utils.shipHasModuleList(ScienceLabParts));
            if (wheelsRequred)
                RenderListItem("Wheels", Utils.shipHasModuleList(new List<string> { "ModuleWheel" }));
            if (scienceRequired)
                RenderListItem("Science", Utils.shipHasModuleList(ScienceParts));

            //DragEnabled = true;
            //GUILayout.Label(new GUIContent("Window Contents", "Here is a reallly long tooltip to demonstrate the war and peace model of writing too much text in a tooltip\r\n\r\nIt even includes a couple of carriage returns to make stuff fun"));
            //GUILayout.Label(String.Format("Drag Enabled:{0}", DragEnabled.ToString()));
            //GUILayout.Label(String.Format("ClampToScreen:{0}", ClampToScreen.ToString()));
            //GUILayout.Label(String.Format("Tooltips:{0}", TooltipsEnabled.ToString()));

            if (GUILayout.Button("Toggle Drag"))
                DragEnabled = !DragEnabled;
            if (GUILayout.Button("Toggle Screen Clamping"))
                ClampToScreen = !ClampToScreen;

            if (GUILayout.Button(new GUIContent("Toggle Tooltips", "Can you see my Tooltip?")))
                TooltipsEnabled = !TooltipsEnabled;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Tooltip Width");
            TooltipMaxWidth = Convert.ToInt32(GUILayout.TextField(TooltipMaxWidth.ToString()));
            GUILayout.EndHorizontal();
            GUILayout.Label("Width of 0 means no limit");

            GUILayout.Label("Alt+F11 - shows/hides window");
        }

        private void RenderListItem(string title, bool present)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(title);
            GUILayout.FlexibleSpace();
            GUILayout.Toggle(present, "");
            GUILayout.EndHorizontal();
        }

        private List<string> AntennaParts = new List<string> 
        { 
            "ModuleDataTransmitter", 
            "ModuleLimitedDataTransmitter", 
            "ModuleRTDataTransmitter",
            "ModuleRTAntenna" 
        };
        private List<string> PowerParts = new List<string> 
        { 
            "ModuleGenerator", 
            "ModuleDeployableSolarPanel", 
            "FNGenerator", "FNAntimatterReactor", 
            "FNNuclearReactor", "FNFusionReactor", 
            "KolonyConverter", "FissionGenerator", 
            "ModuleCurvedSolarPanel" 
        };
        private List<string> DockingParts = new List<string> { "ModuleDockingNode" };
        private List<string> ScienceLabParts = new List<string> { "ModuleScienceLab" };
        private List<string> ScienceParts = new List<string> { "ModuleScienceContainer", "ModuleScienceExperiment","ModuleEnviroSensor"  };
    }
}
