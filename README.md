# NASA_UnityProject
#Thomas Ustica
#March 18 2022

This repo contains the scripts used in the Unity project to plot a 3D geometry from given file.

There are a few things worth noting for this project:

-Most importantly, Unity Engine has a difficult time rendering millions of objects at once. As a result, loading the given geometry (PahtCascade-ASCII.xyz, with over 2,000,000 points) will likely crash the program. I created a resolution setting on the Graph object in the editor, which defaults to a low value so that the program can run. It will only plot a resolution % of the points, which will lower the quality but increase the performance.

-I included a camera movement script for viewing the geometry in the Game window, but I highly suggest using the Scene window instead, as the Game window rendering gets quite finnicky as the resolution is increased. Regardless, the controls are the same (WASDQE).

-The scale of each point can also be changed via a Scale option on the Graph object.

-Lastly, if you want to plot a singular block, use PlotBlockOutline(blocks[0]); in the given place in Graph.cs. This code will be labelled.

I'd like to thank the NASA internships program for this opportunity, and I hope you like this Unity3D demonstration!
