# CFD-Demo

An incompressible Navier-Stokes solver for a Lid Cavity flow problem written in C# and .NET 7 for WinForms. The code uses velocity prediction and
pressure correction after solving the elliptical pressure Poisson equation.

This code is based on the work of Owkes, Barba, Alvarez and Nobe. It uses central differencing throughout with Dirichlet boundary conditions for
velocity and a mix of Dirichlet and Neumann boundary conditions for pressure.

The solution makes it easy to play around with different input variables to see how the solution (and the quality of the solution) changes with
Reynolds Number, timesteps, grid quality, etc. Plotting is done with OxyPlot.
