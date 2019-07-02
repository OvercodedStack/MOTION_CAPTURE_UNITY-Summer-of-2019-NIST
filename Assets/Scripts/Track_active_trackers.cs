using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Track_active_trackers : MonoBehaviour {
    //List of all data 
    private List<Obj_entry> entries;
    //All data structure data
    struct Obj_entry
    {
        public string name;
        public float[] position;
        public float[] quaternion;
    }

    //Print all data in the data structure
    public string print()
    {
        string output = "";
        foreach( Obj_entry i in entries)
        {
            string temp_msg = "";
            temp_msg += i.name;
            foreach (float n in i.position)
            {
                temp_msg += n.ToString();
            }
            foreach (float n in i.quaternion)
            {
                temp_msg += n.ToString();
            }
        }
        return output;
    }


}
