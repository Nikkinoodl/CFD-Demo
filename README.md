# CFD-Demo

A Navier Stokes solver for a Lid Cavity flow problem. WinForms, C# and .NET 7.

This is based on the work of Owkes, Barba, Alvarez and Nobe. It uses central differencing throughout with Dirichlet boundary conditions for
velocity throughout and a mix of Dirichlet and Neuman boundary conditions for pressure.

The solution makes it easy to play around with different input variables to see how the solution (and the quality of the solution) changes with
Reynolds Number, timesteps, grid quality, etc. 
