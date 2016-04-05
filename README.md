Foundoc.Manager
===============

Foundoc manager is simple management applications that you can add to your existing application it runs as OWIN middleware.  It can be added to your application at Startup by calling the UseFounDocManager extension method. 

    app.UseFounDocManager(new FounDocManagerOptions
            {
                DocumentStore = documentStore                
            });

The method takes in an options class which you need to provide the document store you are using for your application.  This is where foundoc manager gets the detail about your document types and indexes. 

The manager uses Nancy to host a single page application (written in angular js) and an api that allows you to search documents by type, key and index and make updates to the raw json of the document.  The application can be viewed at
 
    http://{application-root-url}/foundocmanager 
