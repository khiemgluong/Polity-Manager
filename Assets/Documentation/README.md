### Polity Manager - Manage Factions, Teams & Families

<!-- 1. [Description](#description)
   - [Use Cases](#use-cases)
2. [Requirements](#requirements)
3. [Quickstart](#quickstart)
4. [Public APIs](#public-apis)
5. [Advanced](#advanced)
   - [Families](#families)
   - [Leaders](#leaders)
6. [Credits](#credits)
7. [Glossary](#glossary) -->

- [Description](#description)
  - [Use Cases](#use-cases)
- [Requirements](#requirements)
- [Quickstart](#quickstart)
- [Public APIs](#public-apis)
  - [PolityManager.cs](#politymanagercs)
  - [PolityMember.cs](#politymembercs)
    - [ModifyPolityRelation](#modifypolityrelation)
      - [Parameters](#parameters)
  - [PolityStruct](#politystruct)
- [Advanced](#advanced)
  - [Families](#families)
  - [Leaders](#leaders)
- [Limitations](#limitations)
- [Credits](#credits)
- [Glossary](#glossary)

## Description

Polity Manager is an editor based tool designed to manage relations between polities by a centralized matrix, along with individual family relations through a simple node graph.

The PolityManager singleton contains a _Polity Relation Matrix_, a grid table that displays the relation of one polity to another based on their matrix position, similar to the Unity physics collision matrix.
>The Red Team is allied to the Blue Team, but are enemies to the Orks and the Shogunate.
![Polity Relation Matrix](<PolityManager Relation Matrix.png>)

A Polity<sup>1</sup> also contains a serialized array of Class<sup>2</sup> objects, and each Class object has a List of Faction<sup>3</sup> objects. These serve to departmentalize the various branches or groups of your polity into smaller political units.

To connect these polities to a prefab GameObject, the `PolityMember.cs` component is attached to that GameObject which will now assign it to a created polity, along with their class and faction (note that the class and faction will create a "None" selection, so avoid making a class or faction named "None").
> The Shogunate polity has a Daimyo class, and in that class contains a faction called the Nissan Clan.
![PolityMember](PolityMember.png)

### Use Cases

Polity Manager is suited for games that needs to manage various groups of NPCs, especially when these relationships are a bit more complex, such as when one polity needs to react to an enemy of one or more allies. However, it can also be applicable to simple teams with the OnPolityRelationChanged[^2] callback event to signal all PolityMember gameObjects of their new relation.

## Requirements

Requires the [`Newtonsoft.JSON`](https://www.newtonsoft.com/json) package to work.
Enter `com.unity.nuget.newtonsoft-json` into `Add Package by Git URL` using the Unity UPM

## Quickstart

1. Play the Example Demo.unity Scene
2. Click on the PolityManager GameObject in the hierarchy
3. Click on the grid cells in the _Polity Relation Matrix_ to change RelationType<sup>4</sup> between polities.
   - NPCs will react by targeting enemies or enemies of allies.
4. Hover your mouse over an NPC to view their selected polity and family.

This should demonstrate a very basic implementation of how the PolityManager can control NavMeshAgents with a PolityMember that can react to relationship changes based on their selected polity.
You can open the `NPC_Driver.cs` class inside of Example/Scripts to get a better idea of how the class subscribes to events and how it calls public PolityManager methods.

## Public APIs

### PolityManager.cs

Struct

### PolityMember.cs

#### ModifyPolityRelation

Sets a new relationship status between two polities based on their names, adjusting their relation to either Neutral, Allies, or Enemies.

##### Parameters

| Parameter          | Type             | Description |
|--------------------|------------------|-------------|
| `polityMember`     | `PolityMember`   | The member of the polity initiating the relationship change. |
| `theirPolityName`  | `string`         | The name of the polity that is targeted for the relationship change, retrieved from `polityName` in `PolityMember`. |
| `factionRelation`  | `PolityRelation` | The new relation to set; can be `Neutral`, `Allies`, or `Enemies`. |

### PolityStruct

        public struct PolityStruct
        {
            public string polityName;
            public bool isPolityLeader;
            public string className;
            public bool isClassLeader;
            public string factionName;
            public bool isFactionLeader;
        }

## Advanced

The demo scene presents a vary basic implementation of how the PolityManager could handle NavMeshAgents

### Families

### Leaders

## Limitations

## Credits

Mon of the Tokugawa clan of Tokugawa Shogunate
By Hyakurakuto - CC BY-SA 3.0, <https://commons.wikimedia.org/w/index.php?curid=1056853>

Laurel Wreath free icon
By Freepik - Flaticon License, <https://www.flaticon.com/free-icon/laurel-wreath_6024978>

PBR Ground texture - ambientcg.com

Polity Manager was developed by Khiem Luong (github.com/khiemgluong)

## Glossary

1: Polity - Represents the largest & most important political unit such as a government body, corporation or main team.
2: Class - Represents a social class, government branch, organization, or any large collective corp.
3: Faction - Represents a small and temporary political unit, which can be added and removed at runtime.
