using UnityEngine;
using TPC_Server;

//A cut down version of the original Change robots script which will relay data to the
//server exclusively. 
public class Minimal_change_robots : MonoBehaviour
{
    bool change_robot;                      // The boolean that allows the change of robots 
    float old_time;                         // The old-time flag for time-keeping at a periodic time
    TCP_Server server;                      // The primary server relay point
    public string selected_robot;           // Which robot is being selected as the gameobject's name
    public raycast_collision_19 ray_caster; // The raycast selector for selecting the robot
    private int selection;                  // Which robot is being selected as an integer
    string[] rbt_list = new string[6] { "None", "UR5", "UR10L", "UR10R", "ABBL", "ABBR" };//The following string list are the supported robots.

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
        if (now_time > old_time + 5)// Used for periodically checking which robot is being used
        {
            try
            {
                //Attempts to find a robot
                selected_robot = ray_caster.get_name();

            }
            catch
            {
                //If none found, default to none
                selected_robot = "None";
            }
        }
        //Set directly the name of the selected robot as an integer and return on the TCP server
        server.set_msg("$" + decode_str(selected_robot) + ";#\n");
    }

    string decode_str(string word)
    {
        //Simply converts a string into a name and returns it.
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
