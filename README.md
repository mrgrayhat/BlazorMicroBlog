# .Net 5 Blazor WebAssembly MicroBlog WebApplication

this is practical project made with .Net 5 Blazor web assembly as client app, And Asp.net Core API as server side. 
The goal is to demonstrate the capabilities of the Blazor.Net and wasm. Also explain the possibility of designing and building the user interface of web, desktop and mobile applications with PWA capability.

In this project, I just focused on building the user interface by blazor. So I wrote it with the blazor client side (WASM) model (Asp.NetCore Hosted).

### Screen Shots:
 - Browser full page:
  ![Web Blog - Index](Documents/screenshot/Index_FullPageScreenshot.png?raw=true)
  
 - PWA app (desktop):
  ![PWA Blog - Index](Documents/screenshot/MicroBlog_PwaApp_IndexPage.png?raw=true)
 - PWA app (Mobile):
  ![PWA Blog - Index](Documents/screenshot/PWA_Mobile/Screenshot_20210307-145416_Firefox.jpg?raw=true)
 - Mobile Browser:
  ![Mobile Blog - Index](Documents/screenshot/PWA_Mobile/Screenshot_20210307-143551_SamsungInternet.jpg?raw=true)
 - View Post Page:
  ![View Post](Documents/screenshot/PWA_ViewPostPage.PNG?raw=true)

## How to use:
By default, the program runs under Asp.Net Core Hosted Mode.
#### ** Note: ** The Server App Will Host and Proxy Blazor client app inside it's self. so you don't need to run both projects. Just execute api exe file in windows, or dotnet api.dll file in others like linux.
#### First time you run the project, server app will send Blazor WebAssembly App to you (download & cache in browser). After that, it only sends data transfer requests.
  In No Asp.NetCore Hosted Scenario, you must remove blazor services from server startup file. so you need to run webassembly static files manually (CDN's, Github Pages, etc)

### Install .Net5 SDK

- run server (MicroBlog.Server) project
  - cd src\Server
  - dotnet run
- Open the https://localhost:5001/ in your browser (Edge Or Chrome is recommended. in my test, firefox didn't show pwa install button!)

If you want to test PWA App, You must publish project and run production output. (dotnet publish -c Release)

## Extra notes:
### The Server API :
#### Provide a simple blog service for you. This service provides the following capabilities to your clients:
 - Includes a controller and CRUD (Create, Read, Update, Delete) related functions via the Http protocol and **REST**
 - **Paged** output for receiving large amounts of data (PagedResponse<T>). Such as receiving blog posts based on page and number of records (in JSON format).
  - to get blog post's for example: https://localhost:5001/api/blog?page=1&pageSize=10 , for search in posts: /api/blog/search/{term}
 - **Swagger** API Explorer and Documentation to test and see api features: https://localhost:5001/swagger
 - nswag client code generator & open api config for blazor client (Generate CSharpClient including models and services).
 - Basic Separation of Request(input) and Response(output) models via **Dto's**
  
 ### Blazor WASM (PWA Front-End Application)
 #### Provide Web Assembly client, with the ability to download and run in the user's browser. Server independent
 Include pages for displaying and managing blog posts, Communicate with the server api's
  - A Micro Blog System
    - Design based on Razor components And C# capabilities for ui management / functionality.
      - bootstrap 4 library
      - Reusable Razor components. with model binding.
      - No dependence on javascript, I tried to use C # as much as possible.
    - Ability to paginate list/huge data and calculate total page & items.
    -  Lazy loading/rendering ui and asynchronous communications. such as get images, larg objects.
    - Ability to change the application theme. Between dark and light modes with a simple method. (Alpha, Under Construction)
    - Ability to send and view toast notifications.


  ### To test loading speed and download photos, thumbnail images of posts are randomly generated from an online site, for each post.
  You can add more example post't by click on "Fill with demo data" button. The Blog contain some post's as default.
  
  ### Project TODO List:
  - Pre Release 0.8-beta1: -> Done
  - [x] Add Swagger & API Documentation -> Done
  - [x] Add Nswag client generator -> doing
  - [x] Define the post thumbnail/picture
  - [x] Ability to add new post -> Done
  - [x] Ability to Edit a post -> done
  - [x] Ability to remove a post -> done
  - [x] Ability to view a post -> done
  - [x] Ability to store data into a database (EF Core + sqlite db) -> Done
  - [x] Make Fully Responsive PWA App for desktop & mobiles -> Done
  - [ ] Add Asp.Net Core Hosted Model for easier testing/publishing pwa features -> Done
  - Release Version 1.0:
  - [ ] Ability to upload files
  - [ ] Ability to insert Markups, Content Editor
  - [ ] Ability to insert images/media into the post body
  - [ ] Ability to like a post, increase view per each viewer
  - [ ] Ability to comment in posts
  - [ ] In Memory Caching
  - [ ] Make Fully Responsive PWA App for desktop & mobiles
  - [ ] Implement Authentication & Authorization, Basic Security Concepts
  - [ ] Role Based Accounting
  - Release Version 2.0:
  - [ ] Admin/Management Dashboard
  - [ ] User Dashboard
  - [ ] ReDesign User Interface
  - [ ] InApp Configure Blog Settings
  - [ ] Add Ability for using web assembly publish, Independent of .NET Hosting
