# Vicon motion capture add-on to Modular Android Robot Controller Interface (MACI) for CRPI 

### Introduction 
Part of the larger project related with CRPI and Unity integration with an experimental user interface app for Android. This user interface is intended to operate as part of a larger system incorporating the use of a motion capture system interface. This system would use of a wearable motion capture object and locate objects by observing other objects being tracked as well.

The following image is a representation of this wearable object. 
![alt-text](https://raw.githubusercontent.com/OvercodedStack/CRPI-UI-DOCUMENTATION-Summer-of-2019/master/Images/IMG_0007.JPG)

### Installation process

As Github limits the size of uploads, this project will be limited to only including the source folder. Therefore a manual copy and overwrite of the project has to be done in order to download and install the project. This is the process to do so. 
1.	Start Unity editor
2.	Click on new, name a new project, start it as any type of project 
3.	Navigate to the “Project” tab and right click on the assets folder. Click on “Show in Explorer” 
4.	Delete all the files inside the folder and copy all repository files into the folder
5.	You’ve successfully installed the project into unity

### Usage

To start this app, simply open the unity project in unity and press play in the editor  

Alternatively, you can build the solution via File > Build Settings > Build and Run. This results in a stand-alone app that will transmit the required data to the CRPI middleware. You can then create a shortcut for the created app and place it in a familiar location. 

### A note on messages being sent

This app sends data through a TCP connection on a localhost(127.0.0.1) connection on port 27001. This address can be changed in the **Server Relay point** gameobject. This program is reccomended to be utilized in collaboration with the [CRPI framework](https://github.com/OvercodedStack/CRPI_MIDDLEWARE_INTEGRATION-Summer-of-2019-NIST) program being used to interpret the flags sent. 

This program sends one flag that is an integer that ranges from 0 - 6. The flag represents the following: 

- 0: No robot is being observed
- 1: UR5 is being observed
- 2: UR10L(eft side robot arm) is being observed
- 3: UR10R(ight side robot arm) is being observed
- 4: ABBL(eft side robot arm) is being observed
- 5: ABBR(ight side robot arm) is being observed

The order and robot list being utilized can be changed under the **Minimal_change_robots.cs** script in unity with the 

### Conclusion

Special thanks to the following people during SURF 2019:

- Shelly Bagchi
- Dr. Jeremy Marvel
- Megan Zimmerman
- Holiday Inn SURF Fellows

Thank you all for being the great people you guys are!
