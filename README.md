# QuickVR

__QuickVR__ is the result of our technical research and development in Virtual Reality at the __EventLab__, and offers a series of tools to quickly start developing your own VR application on __Unity__.

The main target is to be able to reuse as much code as possible from our past applications. Therefore, it is not something fixed or closed, but it keeps constantly evolving responding to the new requirements of the applications that we develop, and the new features that __Unity__ introduces.

It is created also taking in consideration the usability, so it is really easy to start using it by anyone with a basic knowledge on __Unity__, as well as it is easy to extend and add new functionalities and customize the existing ones.

__QuickVR__ is built on top of the __Unity XR Framework__ (XR stands for VR/AR), which is an abstraction that exposes the common functionalitis of all the XR devices nativelly supported by __Unity__. It does not need to know about each specific implementation of a specific VR Provider, but it acts directly on the common framework. This way, the core principle of __Unity__ engine “Build once, deploy anywhere” is kept by design.

Our library is focused on more high level features that are necessary in all of the applications that we develop, such as Avatar Tracking, planar reflections, logic workflow, locomotion systems and interaction with the environment.

So the different applications that are built using __QuickVR__, have access to all those features already implemented. Without this library, those features should be implemented from the scratch on every new application, which would increase the production time dramatically. With this approach, we can have a prototype of a new VR application in a matter of hours to days, depending on its complexity.
