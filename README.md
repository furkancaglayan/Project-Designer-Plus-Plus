<p align="center"><img width=60% src="https://github.com/furkancaglayan/Project-Designer-Plus-Plus/blob/main/images/project_designer_card.png"></p>

<div align="center">

[![Release](https://img.shields.io/github/v/release/furkancaglayan/Project-Designer-Plus-Plus)](https://choosealicense.com/licenses/mit/)
[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
[![GitHub Issues](https://img.shields.io/github/issues/furkancaglayan/Project-Designer-Plus-Plus.svg)](https://github.com/furkancaglayan/Project-Designer-Plus-Plus/issues)

</div>

<p align="center"><img width=100% src="https://github.com/furkancaglayan/Project-Designer-Plus-Plus/blob/main/images/project_designer_cover.png"></p>


<h1>Introduction</h1>

**Project Designer+** is a framework that lets you plan and design your Unity3D projects beforehand. Create tasks, assign team member, design your process and create class diagrams.

**Versatile Node Types:** Project Designer boasts four diverse node types tailored to suit various project aspects.

**Extensibility:** Project Designer is built with modularity in mind, allowing users to extend its capabilities and customize workflows according to project requirements. Add new node types, new connection types, new menu options and much more.

**User-Friendly Interface:** Navigate Project Designer's intuitive interface with ease. Drag and drop nodes, rearrange layouts, and interact with project elements effortlessly, making project management a breeze for users of all skill levels.

[![Watch the video](https://raw.githubusercontent.com/furkancaglayan/Project-Designer-Plus-Plus/main/images/project_designer_cover.png)](https://raw.githubusercontent.com/furkancaglayan/Project-Designer-Plus-Plus/main/images/project_designer_intro.mp4)


<h1>Features</h1>

* Framework is completely extensible, one can add new node types, new connection types new menu items and even completely unrelated IDrawables.
* Fully supports undo-redo, including newly added items.
* Has inheritance, assocation, dependence and relation connections.
* Project planning with saveable nodes, take notes, attach images, create UML diagrams.
* Add team members to your project and assign them to tasks.
* Allows creating nodes from assets.
* Supports versions from 2020.3 and higher.

<h1>Node Types</h1>

**Notepad Node:** Lets you write notes, and attach images. Useful for keeping track of information in your projects.<br>
**Task Node:** Creates a task that has due time, explanation, assignees and subtasks.<br>
**Class Node:** Useful for creating UML diagrams. Can be created directly from scripts.<br>
**Dashboard Node:** Overview of the project.<br>


<p  align="center">
  <img alt="Note node" src="https://github.com/furkancaglayan/Project-Designer-Plus-Plus/blob/main/images/note.png" width="200" height="200" />
  <img alt="Task node" src="https://github.com/furkancaglayan/Project-Designer-Plus-Plus/blob/main/images/task.png" width="200" height="200"/>
  <img alt="Class node" src="https://github.com/furkancaglayan/Project-Designer-Plus-Plus/blob/main/images/class.png" width="200" height="200"/>
</p>
<p align="center"><img alt="Dashboard node" src="https://github.com/furkancaglayan/Project-Designer-Plus-Plus/blob/main/images/dashboard.png" width="400"></p>

<h1>How to Extend?</h1>
<h2>How to Add New Node Types?</h2>

The main component of the **Project Designer+** is an abstract class called **NodeBase**. New node types can be overridden by creating a class anywhere in your project and inheriting from NodeBase class.
NodeBase types are collected with reflection in the project after each domain reload. A new menu option will be available for your type when you right-click on an empty space in the editor.
This is pretty much all to it, you will be able to drag, select and interact with the new node after creating one with the context menu.
Here’s an example from one of the built-in node types, **"Notepad"**:

```c#
   //Make sure to mark your classes as Serializable.
   [Serializable]
   public class Notepad : NodeBase
   {
       // override node size
       public override Vector2 MinSize => new Vector2(440, 440);
       public override Vector2 MaxSize =>  new Vector2(440, 720);
       //override icon asset name
       public override string IconKey => "note";
       public override bool CanBeCopied => true;

       protected override int FooterHeight => 10;

       public Notepad(string header) : base()
       {
           HeaderText = header;
       }

       // virtual functions can be overriden to do something when the node is created.
       protected override void OnAddedInternal(IEditorContext context, DrawableCreationType drawableCreationType)
       {
           if (drawableCreationType == DrawableCreationType.Default)
           {
               AddMember<CommentMember>();
           }
       }
   }

```

<h2>How to Add New Connection Types?</h2>

Very similarly to the NodeBase, all connections are created from ConnectionBase abstract class. Defining the class is a simple process:

```c#
public class InheritanceConnection : ConnectionBase
 {
     protected override void DrawConnection(IEditorContext context, Vector2 fromOutputScreenPos, Vector2 toInputScreenPos, Vector2 fromCenterScreenPos, Vector2 toCenterScreenPos, Color color)
     {
         Vector2 start = fromOutputScreenPos + (toInputScreenPos - fromOutputScreenPos).normalized * 10;
         Vector2 end = toInputScreenPos - (toInputScreenPos - fromOutputScreenPos).normalized * 20;
         GUIUtilities.DrawLine(start, end, color);
         GUIUtilities.DrawTriangle(end, 15f, color, toInputScreenPos - fromOutputScreenPos, false);

         Vector2 midPoint = (fromOutputScreenPos + toInputScreenPos) / 2;
         GUI.Label(new Rect(midPoint, new Vector2(60, 18)), "inherits");
     }
 }

```
