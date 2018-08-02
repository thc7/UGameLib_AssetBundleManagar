using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Libs{

    public class CfgItem{

        public string cmd;
        public string value;

        public CfgItem(string cmdp,string valuep){
            cmd = cmdp;
            value = valuep;
        }
    }

    public class LibsToolsCFG{

        public HashSet<CfgItem> cfgItemArr = new HashSet<CfgItem>();

        public LibsToolsCFG(){
            
            string fileAddress = System.IO.Path.Combine(Application.dataPath + "/", "Libscfg");

            FileInfo fInfo0 = new FileInfo(fileAddress);
            string cfg = "";
			if (fInfo0.Exists) {
				StreamReader r = new StreamReader (fileAddress);
				cfg = r.ReadToEnd ();
			} else {
				EditorUtility.DisplayDialog("错误！", Application.dataPath + "/" +"Libscfg 不存在！请从libs下copy", "ok");
				return;
			}

            string[] lines = cfg.Split(new char[]{'\n'});

            for(int i = 0; i< lines.Length; i++)
            {
                string line = lines[i];
                if (line == "" && line.Equals(""))
                {
                    continue;
                }
                if (line.StartsWith("//"))
                {
                    continue;
                }
                string key = line.Substring(0, line.IndexOf("="));
                string value = line.Substring(line.IndexOf("=")+1);

                CfgItem cfgItem = new CfgItem(key,value);
                cfgItemArr.Add(cfgItem);
            }
        }

        public string GetPathList(string cmd){

            string str = "";
            foreach( CfgItem f in cfgItemArr){
                if(cmd.Equals(f.cmd))
                    str+= f.value + "\n";
            }
            return str;
        }

    }

    public class LibsTools  {
        
        static public void CopyOut(string cmd,string file,LibsToolsCFG libsToolsCFG ){

            foreach(CfgItem cfgItem in libsToolsCFG.cfgItemArr){ 
                if (cfgItem.cmd.Equals(cmd))
                {
                    FileTools.CopyDir(file, cfgItem.value);
                }
            }
            EditorUtility.DisplayDialog("LOG", "Copy 完成！", "ok");
        }
        /*
        [MenuItem("Assets/LibsTools/ConvertLineEndingsMacOS_UNIX")]
        static public void ConvertLineEndingsMacOS_UNIX(){

            string Path = System.IO.Path.Combine(Application.dataPath ,"Libs/Manager/AssetBundleManagar.cs" );

            string str = System.IO.File.ReadAllText(Path,System.Text.Encoding.UTF8);

            str = str.Replace('\r','\n');
            str = str.Replace("\n\n", "\n");

            System.IO.File.WriteAllText(Path,str);
        }
        */
        [MenuItem("Assets/LibsTools/Copy Libs To Other")]
        static public void CopyLibsToOther(){

            LibsToolsCFG libsToolsCFG = new LibsToolsCFG();

            if (EditorUtility.DisplayDialog("LOG", "确定将 Libs Copy 到 " + libsToolsCFG.GetPathList("CopyTo"), "ok", "cancel")){
                CopyOut("CopyTo", Application.dataPath + "/Libs" , libsToolsCFG);
            }

        }

		[MenuItem("Assets/LibsTools/Copy Assets To Other")]
		static public void CopyAssetsToOther(){

			LibsToolsCFG libsToolsCFG = new LibsToolsCFG();

			if (EditorUtility.DisplayDialog("LOG", "确定将 Assets Copy 到 " + libsToolsCFG.GetPathList("CopyAssetsTo"), "ok", "cancel")){
				CopyOut("CopyAssetsTo", Application.dataPath  , libsToolsCFG);
			}
			if (EditorUtility.DisplayDialog("LOG", "确定将 ProjectSettings Copy 到 " + libsToolsCFG.GetPathList("CopyProjectSettingsTo"), "ok", "cancel")){
				CopyOut("CopyProjectSettingsTo", Application.dataPath.Replace("Assets","ProjectSettings")  , libsToolsCFG);
			}
		}
         
		[MenuItem("Assets/LibsTools/Copy ProjectSettings To Other")]
		static public void CopyProjectSettingsToOther(){

			LibsToolsCFG libsToolsCFG = new LibsToolsCFG();

			if (EditorUtility.DisplayDialog("LOG", "确定将 ProjectSettings Copy 到 " + libsToolsCFG.GetPathList("CopyProjectSettingsTo"), "ok", "cancel")){
				CopyOut("CopyProjectSettingsTo", Application.dataPath.Replace("Assets","ProjectSettings")  , libsToolsCFG);
			}

		}

        [MenuItem("Assets/LibsTools/Copy StreamingAssetsPath To Other")]
        static public void CopyStreamingAssetsPathToOther(){

            LibsToolsCFG libsToolsCFG = new LibsToolsCFG();

            if (EditorUtility.DisplayDialog("LOG", "确定 Copy 到 " + libsToolsCFG.GetPathList("StreamingAssetsTo"), "ok", "cancel")){
                CopyOut("StreamingAssetsTo", Application.streamingAssetsPath , libsToolsCFG);
            }

        }

    }

}