# QuickVR

__QuickVR__ is the result of our technical research and development in Virtual Reality at the __EventLab__, and offers a series of tools to quickly start developing your own VR application on __Unity__.

The main target is to be able to reuse as much code as possible from our past applications. Therefore, it is not something fixed or closed, but it keeps constantly evolving responding to the new requirements of the applications that we develop, and the new features that __Unity__ introduces.

It is created also taking in consideration the usability, so it is really easy to start using it by anyone with a basic knowledge on __Unity__, as well as it is easy to extend and add new functionalities and customize the existing ones.

__QuickVR__ is built on top of the __Unity XR Framework__ (XR stands for VR/AR), which is an abstraction that exposes the common functionalitis of all the XR devices nativelly supported by __Unity__. It does not need to know about each specific implementation of a specific VR Provider, but it acts directly on the common framework. This way, the core principle of __Unity__ engine “Build once, deploy anywhere” is kept by design.

Our library is focused on more high level features that are necessary in all of the applications that we develop, such as Avatar Tracking, planar reflections, logic workflow, locomotion systems and interaction with the environment.

So the different applications that are built using __QuickVR__, have access to all those features already implemented. Without this library, those features should be implemented from the scratch on every new application, which would increase the production time dramatically. With this approach, we can have a prototype of a new VR application in a matter of hours to days, depending on its complexity.

This work is funded by the European Research Council (ERC) Advanced Grant Moments in Time in Immersive Virtual Environments (MoTIVE) 742989.

# Install

First of all you need to install __XR Plug-in Management__ if you have not installed it yet on your project. Go to _Edit > Project Settings > XR Plug-in Management_ and select _Install XR Plug-in Management_. 

![](/Documentation~/img/install/000.png)

Go to _Window > Package Manager_ and click on the ‘+’ symbol in the top left corner of the new window. Select _Add package from git URL…_

![](/Documentation~/img/install/00.png)

A text field will open. Copy and paste the following URL, and then click on _Add_. 

https://github.com/eventlab-projects/com.quickvr.quickbase.git

__Now be patient__. It seems that Unity does not produce any kind of visual feedback and it looks like nothing is happening, but the package is downloading. Then it will be automatically imported. 

Once this process is done, the following window may appear, depending on the settings of your project. 

![](/Documentation~/img/install/01.png)

If you’re starting a new project, just select _Yes_ and ignore the following sentence. Otherwise, if you are introducing the QuickVR library in an existing project and you want to keep support for legacy’s Unity input system, select _No_, and go to _Edit > Project Settings > Player_ and set _Active Input Handling_ to _Both_. 

If you want, you can install some samples by going to _Window > Package Manager_. You’ll see that a new tab called __EventLab__ has appeared, containing the package __QuickVR.QuickBase__. 

![](/Documentation~/img/install/02.png)

Select _Import_ on those samples you want to install. The samples are installed in _Assets > Samples_.  

Done! Follow the documentation on the _Samples_ folder and the documentation on the wiki to learn how to configure your application for VR and start using the library. 

