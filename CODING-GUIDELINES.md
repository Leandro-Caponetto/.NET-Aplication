# Coding Guidelines

[TOC]

## Arquitectura de la solución

### Tecnologías utilizadas

- .NET Core 2.2
- ASP.NET CORE MVC
- MediatR
- EntityFramework
- Autofac
- Serilog

Todas las librerías externas se instalaron mediante [Nuget](https://www.nuget.org/).

## Api

### Estructura de proyectos en la solución

Para el desarrollo se siguió el principio de [DIP](https://en.wikipedia.org/wiki/Dependency_inversion_principle) (Dependency Inversion Principle), lo que resulta en la inversión del clásico arbol de depencias donde los módulos "mayores" dependen de los "menores".

### Detalles de la implementación (Inyección de dependencias)

Al seguir el DIP, es necesario definir las dependencias que tendría un proyecto como `interfaces` para que luego otro proyecto que las implemente pueda hacer referencia al primero y así invertir el árbol de dependencias.

El beneficio es que ahora el sistema no está acoplado a una implementación específica y los componentes se pueden intercambiar de ser necesario (por ejemplo, en este proyecto hacer el reemplazo de los fakes por las implementaciones reales).

Para poder definir que implementaciones se van a utilizar es necesario un [IoC Container](https://martinfowler.com/articles/injection.html), en este caso nosotros elejímos [Autofac](https://autofac.org/).

El lugar donde se realiza la configuración de las implementaciones se denomina [CompositionRoot](https://stackoverflow.com/a/6277806), en este caso el `Statup.cs` inicia este proceso utilizando la clase `BootstrapperModule.cs`.

En `BootstrapperModule.cs` se cargan `Modules` que finalmente especifican las implementaciones y sus ciclos de vida.

### CQRS (Command Query Responsability Segregation)

La arquitectura `CQRS` divide las operaciones del sistema en `Commands` y en `Queries`, los primeros modifican estado ([Side-Effects](https://en.wikipedia.org/wiki/Side_effect_(computer_science))) mientras que los segundos sólo obtienen información de la base de datos.

De esta manera se logra segregar en clases lo que comúnmente sería una arquitectura del tipo Transaction Script a pequeñas clases que respetan los principios [SOLID](https://en.wikipedia.org/wiki/SOLID_(object-oriented_design)).

Los siguientes esquemas muestran como es la interacción de los componentes del modelo `CQRS` para `Commands` y `Queries` respectivamente.

#### Commands

![CQRS_Commands](diagrams/coding-guidelines/CQRS_Commands.png)

Los `CommandHandler` se encargan de orquestar la obtención de la `Entity` a partir del `Repository` y luego realizar las validaciones y modificaciones necesarias en la `Entity`. Luego, existe un decorator sobre los `CommandHandler` que se encarga de persistir esa "Transacción".

![Command Flow](diagrams/coding-guidelines/CommandFlow.png)

#### Queries

![CQRS_Queries](diagrams/coding-guidelines/CQRS_Queries.png)

Los `QueryHandler` sólo son pasamanos entre lo que pide la UI y el `Repository`.

Los `Command`, `CommandHandler`, `Query` y `QueryHandler` están definidos en `Core` o `Core.Data.EF` según el modulo y operación (por ejemplo. account/login)

### Core

En el proyecto **Core** se definen todas las abstracciones necesarias que luego serán utilizadas para darle funcionamiento al sistema. 

### Data Persistence

Para resolver la capa de datos y la persistencia se utiliza **Entity Framework** en su modalidad `Code-First`.

Dentro del proyecto se definen los `EntityTypeConfiguration`, los `Migration` y se encuentra la implementación de los `Repository`.

Los `EntityTypeConfiguration` son clases que le definen a `Entity Framework` de cómo persistir una `Entity`.

Los `Migration` son clases que definen cómo implementar un cambio en el modelo de datos, éstas hay que generarlas cada vez que se modifica una `Entity` o se realiza algún cambio en un `Mapping`. Para generarlas, ejecutar el comando `add-migration <nombre-migration>` en el `Package Manager Console` del visual studio. Luego para impactar los cambios descritos en el `Migration` se debe ejecutar el comando `update-database` y los cambios se reflejarán en la base de datos.

Es importante que al momento de ejecutar los comandos, este seleccionado como **startup project** `Presentation.Api` y como **default project** `Core.Data.EF`, de otro modo, la consola arrojará errores.
