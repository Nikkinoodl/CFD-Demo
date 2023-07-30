# CFD-Demo

An incompressible Navier-Stokes solver for a Lid Cavity flow problem written in C# and .NET 7 for WinForms. The code uses a finite difference method 
with the SIMPLE algorithm (velocity prediction and pressure correction after solving the elliptical pressure Poisson equation).

This code is based on the work of Owkes, Barba, Alvarez and Nobe. It uses central differencing throughout with Dirichlet boundary conditions for
velocity and a mix of Dirichlet and Neumann boundary conditions for pressure.

The solution makes it easy to play around with different input variables to see how the solution (and the quality of the solution) changes with
Reynolds Number, timesteps, grid quality, etc. Plotting is done with OxyPlot.

Sample results using the application default settings:

![U Velocities in Lid Cavity - Re 100](https://user-images.githubusercontent.com/17559271/227782456-43415f81-02a1-4578-99cd-71cf4558c533.jpg)
![V Velocities in Lid Cavity - Re 100](https://github.com/Nikkinoodl/CFD-Demo/assets/17559271/f538b894-a62d-4a4b-a473-c45630bb301a)
![Pressure P in Lid Cavity - Re 100](https://github.com/Nikkinoodl/CFD-Demo/assets/17559271/194297c3-f4ad-4670-8712-f60d8f6fc81f)
![U Velocities at Mid Line - Re 100](https://user-images.githubusercontent.com/17559271/227782467-e2a180be-4c97-4b5c-b6c0-6815a8900f36.jpg)
