# DocGen
 *Lightweight markdown documentation generator for Unity.*



**DocGen** was created to generate simple and clean *markdown* pages that can be used as a starting point to further document your Unity project.

The ideal scenario is to pair it with a framework like [MkDocs](https://www.mkdocs.org/) so that you can get your project's documentation up and running in no time.



## How to use

- Import the UnityPkg into your game. 

- Use the **Docs** tag to add documentation to your:

  - Classes
  - Structs
  - Methods
  - Variables
  - etc...

  

  Like so:

  ```csharp
  [Docs("Write your documentation here")]
  public int_mMyVar = 10;
  ```

  

- Create a new **DocumentationSettings** file from the create asset menu.

- Specify which classes **DocGen** is supposed to document (write the class names in the settings file).

- Click **Generate** and enjoy your new documentation.



## License

DocGen is licensed under the [MIT License](https://opensource.org/licenses/MIT).

