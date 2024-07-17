# Introduction 

Plugin for TiendaNube

## Getting Started

Para desarrollar y ejecutar la app es necesario tener instalado:

- Visual Studio 2019
- .Net Core 3.1
- **Node** >= 8.12.0: se puede descargar desde [aquí](https://nodejs.org/en/).
  - Ejecutar en el root del repo en una consola: npm install

## ¿How to Contribute?

Leer [CONTRIBUTE.md](CONTRIBUTE.md).

## Branching Model

Leer [BRANCH-MODEL.md](BRANCH-MODEL.md).

## Coding Guidelines

Leer [CODING-GUIDELINES.md](CODING-GUIDELINES.md).

## Environments

Se crearon los siguientes ambientes:

- **[DEV](http://dev.....originsw.com/swagger)**: refleja hasta el último commit en el branch de `development` (en este caso **master**).
- **[QA](http://qa.....originsw.com/swagger)**: refleja hasta el último commit en el branch de `release` que se encuentre creado.
- **[PREPROD](http://preprod.....originsw.com/swagger)**: refleja hasta el último commit en el branch de `release` o `hotfix` que se encuentre creado.
- **[PROD](http://www.....originsw.com/swagger)**: refleja hasta el último commit en el branch de `production` (en este caso **prod**).

**Todos los ambientes tienen CI/CD (continuous integration y continuous deployment).** 

La configuración de dichos ambientes puede ser accedida solamente por aquel que tenga el rol de administrarlos y **NO se debería realizar ningún deployment manual**.

## Deployments

Se implemento la siguiente cadena de ambientes:

- **DEV** -> **QA** -> **PREPROD** -> **PROD**

El circuito para realizar los deploys es el siguiente:

- **PASAJE A DEV:**
  Cada vez que se realiza un **push a master**, se genera un build, se instala en DEV.

- **PASAJE A QA:** 
  Cuando este listo para pasar a QA, el encargado de **DEV debe realizar la aprobación** para que pase a dicho ambiente. **Además debe crear el tag x.y.z-beta-i correspondiente.**
  La nomenclación del tag es **x.y.z** para la versión e **i** para el número de iteración ya que pueden encontrarse bugs durante este proceso.
  Para ser **instalado en QA, el encargado de QA debe realizar la aprobación correspondiente.**
- **PASAJE A PREPROD:** 
  Cuando este listo para pasar a PREPROD, el encargado de **QA debe realizar la aprobación** para que pase a dicho ambiente.
- **PASAJE A PROD:** 
  Cuando este listo para pasar a PROD, el **encargado del proyecto debe realizar la aprobación** para que pase a dicho ambiente y le debe **informar a desarrollo para que cree el tag x.y.z a partir del tag x.y.z-beta generado en los pasos anteriores y elimine el tag beta de esa versión.**

 

### BUG-FIXES

En caso de encontrar problemas en la etapa de QA de un release, generar el branch release/x.y.z correspondiente y realizar los fixes ahí mismo. 

Al momento de pushear CI/CD deberá generar un build correspondiente y el flujo de deploy se inicia desde **PASAJE A QA**.

 

### HOT-FIXES

En caso de encontrar problemas en producción, generar el branch hotfix/name correspondiente y realizar los fixes ahí mismo. Al momento de pushear CI/CD deberá generar un build correspondiente y el flujo de deploy se inicia desde **PASAJE A PREPROD.**

 

**TENER EN CUENTA**

Importante, siempre hacer un push a master cualquier sea el cambio, por mas mínimo que sea al terminar el día.
El Pending approval de dev, es para que las push no pasen automáticamente a QA, la idea es que se vayan realizando los cambios en dev auque sean mínimos y el ultimo dev se apruebe para no estar molestando al QA por cambios que no sean significativos en la versión



### IMPORTANTE

Debido a limitaciones de AzureDevOps, se prevee crear un pipeline de CD por cada camino posible en el deploy, es decir, por cada repositorio (Frontend, Backend y API) existirán tres pipelines:

 **FROM DEV:**

En caso de que el build sea pusheado a DEV se comienza desde **PASAJE A DEV.**

**FROM QA:** 

Este pipeline se utiliza en caso de que el build sea proveniente de un **BUG-FIX,** se comienza desde **PASAJE A QA.**

 **FROM PREPROD:**

Este pipeline se utiliza en caso de que el build sea proveniente de un **HOT-FIX,** se comienza desde **PASAJE A PREPROD.**