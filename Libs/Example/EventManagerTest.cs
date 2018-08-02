using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManagerTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    bool OnEvent(string name,object data){
        Debug.LogFormat(" On Event Name = {0},data = {1}",name,data);
        return false;
    }

    void OnGUI(){
        
        if(GUI.Button(new Rect(0,0,78,24),"Add"))
        {
            Libs.EM.I.Add("EventName", OnEvent);
        }

        if (GUI.Button(new Rect(0, 24, 78, 24), "Send"))
        {
            Libs.EM.I.Send("EventName", this);
        }

        if (GUI.Button(new Rect(0, 24 * 2, 78, 24), "Rem"))
        {
            Libs.EM.I.Rem("EventName", OnEvent);
        }

        if (GUI.Button(new Rect(0, 24 * 3, 78, 24), "Rem By Target"))
        {
            Libs.EM.I.RemByTargetAll(this);
        }
    }
       
    void OnDestroy(){

        if (!Libs.EM.isNULL())
        {
            Libs.EM.I.RemByTargetAll(this);
        }
    }
}
