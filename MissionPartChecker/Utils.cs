using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MissionPartChecker
{
    class Utils
    {
        public static bool shipHasModuleList(List<string> moduleList)
        {
            //This checks a list of modules, prevents us from having to loop over the vessel multiple times.

            var editorParts = EditorLogic.SortedShipList;
   
            foreach (Part part in editorParts)
            {
                foreach (PartModule module in part.Modules)
                {
                    //We must go deeper.
                    foreach (string checkModule in moduleList)
                    {
                        if (module.moduleName == checkModule || module.ClassName == checkModule)
                            return true;
                    }
                }
            }
      
            return false;
        }
    }
}
