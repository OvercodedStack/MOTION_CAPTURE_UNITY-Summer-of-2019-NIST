using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TPC_Server;
using UnityEngine.UI;


//A cut down version of the original Change robots script which will relay data to the
//server exclusively. 
public class Minimal_change_robots : MonoBehaviour
{
    bool change_robot;
    float old_time;
    TCP_Server server; 
    public string selected_robot;
    public raycast_collision_19 ray_caster;
    private int selection;
    string[] rbt_list = new string[6] { "None", "UR5", "UR10L", "UR10R", "ABBL", "ABBR" };

    // Use this for initialization
    void Start()
    {
        server = GetComponent<TCP_Server>();
        selected_robot = "None";
        old_time = Time.time;
    }


    //Get the currently selected robot and send it out for relay to the server. 
    void Update()
    {
        float now_time = Time.time;
        if (now_time > old_time + 5)
        {
            try
            {
                selected_robot = ray_caster.get_name();

            }
            catch
            {
                selected_robot = "None";
            }
        }
        server.set_msg(decode_str(selected_robot) + ";\n");
    }

    string decode_str(string word)
    {
        switch (word)
        {
            case "UR5":
                return "1";
            case "UR10L":
                return "2";
            case "UR10R":
                return "3";
            case "ABBL":
                return "4";
            case "ABBR":
                return "5";
            default:
                return "0";
        }
    }
    //Get the robot (manual override); 
    public string get_robot()
    {
        return selected_robot;
    }
}
