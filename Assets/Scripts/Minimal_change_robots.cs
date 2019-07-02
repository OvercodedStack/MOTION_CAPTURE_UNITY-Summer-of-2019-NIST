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
        change_robot = false;
        old_time = Time.time;
    }


    //Get the currently selected robot and send it out for relay to the server. 
    void Update()
    {
        float now_time = Time.time;
        if (now_time > old_time + 5 && change_robot)
        {
            change_robot = !change_robot;
            }
        else if (change_robot)
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
        server.set_msg(selected_robot);
    }

    //Get the robot (manual override); 
    public string get_robot()
    {
        return selected_robot;
    }
}
