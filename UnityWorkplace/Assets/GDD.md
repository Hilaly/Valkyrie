# Snake The Game.

Snake is a video game genre where the player maneuvers a growing line that becomes a primary obstacle to itself. The concept originated in the 1976 two-player arcade game Blockade from Gremlin Industries, and the ease of implementation has led to hundreds of versions (some of which have the word snake or worm in the title) for many platforms. 1982's Tron arcade game, based on the film, includes snake gameplay for the single-player Light Cycles segment. After a variant was preloaded on Nokia mobile phones in 1998, there was a resurgence of interest in snake games as it found a larger audience.

## Entities:
- Wall
- Food
- Snake
- Head
- Body.

## Different properties.

position is `UnityEngine.Vector2Int` property.
input is `UnityEngine.Vector2Int` property.

Snake contains Head.
Snake contains list of Body.

Head has position.
Body has position.

## Logic.

Every tick Snake readInput.
Every 0.5 seconds we:
- move Snake
- check lose.

readInput is change input to `new UnityEngine.Vector2Int(1,0)`.
move is change position by `input*dt`.

## Tasks that need to be done:
- [x] Write log action
- [x] Skip action
- [x] Create type action
- [x] Add base type action
- [x] Add attribute action
- [x] Add method action
- [ ] Add code to method action
- [x] Add property action
- [x] Add code to property set action
- [x] Add code to property get action
- [x] Set global var action
- [x] Set local var action
- [ ] Get global var action
- [x] Push local up action
- [x] if action
- [x] if else action
- [ ] Blackboard
- [x] Code snippets
- [x] Add code to class direct
- [x] Alias

.
<!--

We have very little time:
And if we use direct lines, then we have one paragraph.

***
---


This is [an example] [id] reference-style link.

This is [an example](http://example.com/ "Title") inline link.

[This link](http://example.net/) has no title attribute.

[id]: http://example.com/  "Optional Title Here"

``There is a literal backtick (`) here.``

Use the `printf()` function.

*single asterisks*

_single underscores_

**double asterisks**

__double underscores__

# A First Level Header

## A Second Level Header

Now is the time for all good men to come to
the aid of their country. This is just a
regular paragraph.

The quick brown fox jumped over the lazy
dog's back.

### Header 3

> This is a blockquote.
>
> This is the second paragraph in the blockquote.
>
> ## This is an H2 in a blockquote

* Paragraph
    
    with lines
* sdfkds

- [x] 739
- [x] https://github.com/octo-org/octo-repo/issues/740
- [ ] Add delight to the experience when all tasks are complete :tada:

<details><summary>CLICK ME</summary>
<p>

#### We can hide anything, even code!

```c#
   System.Console.WriteLine("Hello World");
```

</p>
</details>

Here is a simple footnote[^1].

A footnote can also have multiple lines[^2].

You can also use words, to fit your writing style more closely[^note].

[^1]: My reference.
[^2]: Every new line should be prefixed with 2 spaces.  
This allows you to have a footnote with multiple lines.
[^note]:
Named footnotes will still render with numbers instead of the text but allow easier identification and linking.  
This footnote also has been made with a different syntax using 4 spaces for new lines.

###### This is comment in language dictionary and we can see it in markdown.

```
<root>:
  => $name
  => log name.
```

```
<entities_creation>:
  => "Entities :" <entities_list>
  => log "entities parsed", call <entities_list>.
```

.
-->