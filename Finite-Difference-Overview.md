# Solving the Navier-Stokes Equations

Starting with Newton's Second Law of motion and applying it to an incompressible fluid, we can derive two equations that describe the motion of the fluid. These are known as the Navier-Stokes equations:

$$\begin{equation}\frac{\partial \textbf{u}}{\partial t} + \textbf{u}\cdot{\nabla}\textbf{u} = -{\frac{1}{\rho}}{\nabla}{p} + \nu{\nabla}^2\textbf{u}\end{equation}$$

$$\begin{equation}\nabla \cdot \textbf{u} = 0\end{equation}$$

In this document we will be dealing with two-dimensional
fluid flow, so in these and subsequent equations the variables repesent **u** = [u, v], a two-dimensional velocity vector, t is time, $$\rho$$ is density,
p is pressure, and $$\nu$$ is the kinematic viscosity.

## Finite Difference

There are a mere handful of cases where we can obtain an exact solution
for these equations, and no general solution is known to exist. However, there are
several ways to solve them numerically, each of which involves
some amount of simplification or compromise.

One of the most widely used methods involves breaking up the domain into
a grid of evenly sized rectangular elements and developing a solution by using the
grid's geometry to approximate the derivatives of u and p at the grid's nodes.

The diagrammatic representation of the grid shown below is commonly referred to as a stencil.

![Stencil of the discretization grid](FVM-1.png)

On a stencil like this one we can then derive approximate expressions for derivatives as:

$$\frac{\partial u}{\partial x} \simeq  \frac {u_{i+1, j} - u_{i, j}}{\Delta x}$$

and second derivatives as:

$$\frac{\partial u}{\partial x} \simeq  \frac {u_{i-1, j} - 2u_{i, j} + u_{i+1, j}}{\Delta x^2}$$

We do the same thing for the vertical component of velocity v, and also for pressure p. It is obvious that the accuracy of this approximation increases as the grid becomes smaller.

While these approximations are reasonably straightforward, the Navier-Stokes equations are non-linear in $$\mathbf u$$ and cannot be solved directly using an $$A \mathbf x = b$$ type solver. Instead, we must solve them iteratively.

### Setting Boundary Conditions

The solution of the Navier-Stokes equation begins by setting Dirichlet conditions at the domain boundary. These are essentially  fixed values for the u and v components of **u** which remain unchanged in the subsequent calculations. We also set a nominal initial value for p at node index [0,0].

### Temporal Discretization

After setting Dirichlet conditions, we start to propogate velocity and pressure propagate the domain in steps of time until they (hopefully) become stable in value.

At the nth timestep we have:
$$\begin{equation}\frac{\mathbf{u}^{n+1}-\mathbf{u}^n}{\Delta t}=-\frac{1}{\rho}\nabla p^{n+1} - \mathbf{u}^n \cdot \nabla \mathbf{u}^n + \nu \nabla ^2 \mathbf{u}^n \end{equation}$$

Which can be rewritten as:

$$\begin{equation}\mathbf{u}^{n+1}=\left( -\frac{1}{\rho}\nabla p^{n+1}  - \mathbf{u}^n \cdot \nabla \mathbf{u}^n + \nu \nabla ^2 \mathbf{u}^n \right)\Delta t + \mathbf{u}^n \end{equation}$$

### The SIMPLE Method

Equation (1) represents the conservation of momentum. Equation (2) represents mass conservation at constant
density. Unfortunately, there is no pressure term in the incompressible continuity equation to provide a relation between velocity and pressure (in compressible flow the mass continuity provides an equation for the density $$\rho$$, which we can use along
with an equation of state that relates density and pressure). 

The workaround is to use a predictor-corrector method which first solves for velocity in the absence of pressure, then corrects for pressure in a second step.

### The Predictor Step

Dropping the pressure term and rewriting $$\mathbf{u}^{n+1}$$ as $$\mathbf{u}^*$$ gives the predictor
equation:

$$\begin{equation}\mathbf{u}^*=\left(- \mathbf{u}^n \cdot \nabla \mathbf{u}^n + \nu \nabla ^2 \mathbf{u}^n \right)\Delta t + \mathbf{u}^n \end{equation}$$

Where:

$$\begin{equation}\frac{\mathbf{u}^{n+1}-\mathbf{u}^*}{\Delta t}=-\frac{1}{\rho}\nabla p^{n+1}\end{equation}$$


### The Pressure Poisson Equation

We can then separately solve for $$\mathbf{u}^*$$:

$$\begin{equation}\nabla \big( \frac{\mathbf{u}^{n+1}-\mathbf{u}^*}{\Delta t} \big)=-\nabla \big(\frac{1}{\rho}\nabla p^{n+1}\big)\end{equation}$$

By continuity:

$$\begin{equation}\nabla \cdot \mathbf{u}^{n+1} = 0\end{equation}$$

Therefore:

$$\begin{equation}\nabla ^2 p^{n+1} = \frac{\rho}{\Delta t} \nabla \cdot \mathbf{u}^*\end{equation}$$

### The Corrector Step and von Neuman Boundary Condition Reset

After separately calculating a value for $$\mathbf{u}^*$$, we substitute it back into equation (5).

Before proceeding to the next timestep, we first set von Neuman boundary conditions—the derivatives of u and v at the boundary are set to 0 by adjusting the value of u and v at the nodes adjacent to the boundary to match those at the boundary nodes.

## Further Reading

Most of the coding in this project was derived by working though the algorithms discussed by Alvarez and Nobe [1]. I have followed their methodology, particularly in using central differencing throughout and simplifying the treatment of the pressure calculation and setting of boundary conditions

That said, I first became interested in developing this project by reading and working through Owkes [2] and then working though the steps provided by Lorena Barba [3].
1. [Alvarez and Nobe](https://colab.research.google.com/github/josealvarez97/The-Ultimate-Guide-to-Write-Your-First-CFD-Solver/blob/main/The_Ultimate_Guide_to_Write_Your_First_CFD_Solver.ipynb)

2. [Owkes](https://www.montana.edu/mowkes/research/source-codes/GuideToCFD.pdf)

3. [Barba et al.](https://github.com/barbagroup/CFDPython/blob/master/lessons/14_Step_11.ipynb)



