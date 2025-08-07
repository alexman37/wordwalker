## Wordwalker - From Start to Finish
In this writeup, I'll explain the entire process of how I developed Wordwalker over the course of 5 months - everything from "Hello World" to releasing the final build! It's going to be a long one, so skip around to wherever interests you most, if you like.

### Table of Contents
1. [Background and Concept](#background-and-concept)
2. [Tilemap Generation](#tilemap-generation)
3. [Gameplay](#gameplay)
4. [Game State](#game-state)
5. [Word Lists](#word-lists)
6. [UI Design](#ui-design)
7. [Art](#art)
8. [Persistent Storage](#persistent-storage)
9. [Bugfixing, Playtesting and Release](#bugfixing-playtesting-and-release)
10. [Conclusion](#conclusion)

### Background and Concept
The idea for Wordwalker randomly came to me in November of 2024 when I was taking a shower. I thought of a scene from Indiana Jones 3, when Indy is solving a puzzle in a temple. I have absolutely no idea why I remembered this.

The clip starts at 1:40:
https://www.youtube.com/watch?v=XqGWI0WTj24

<img width="1009" height="440" alt="image" src="https://github.com/user-attachments/assets/6d0ab255-8dd7-42ae-94f9-c3d72f41f4e8" />\
<em>The puzzle in question, "Spell the name of God". Indy has to walk across letter tiles that spell out a secret word, to get from one side to another. The fake tiles collapse into an endless chasm.</em>

I thought it was a great idea for a word game, but wondered if someone had already made it by then. To my delight, no one had!

At that time in my life, I'd used Unity for a while on several projects, including another project with Hexagon generation called Island Hopping Simulator. I'd also been doing full-stack development as a
full time job for over a year. I felt confident I could do this. My only worry was that I'd never "completely finished" a game before, doing everything I wanted to do, and only then releasing it to the world.

So that became my main goal: to make a game I was genuinely proud of and see it all the way through. There were road bumps early on, but after enough skeleton structure was in place, its development actually came
very naturally. I'm very satisfied with the end product and I hope you enjoy it too.

A couple things made the difference. I'll go into more detail below, but to summarize:
- I shared regular progress updates with my friends and family to get honest feedback from them.
- I kept the core concept of the game simple and consistent throughout the entire development process.
- I planned a lot of this out on paper. Like, a LOT.

<img width="1384" height="1038" alt="image" src="https://github.com/user-attachments/assets/3c1b12ae-0ec9-4edf-a527-2d962eca4b41" />\
<em>There's about 2 times more pages than what you see.</em>

### Tilemap Generation
Source code:
- [TilemapGen](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Manager/TilemapGen.cs): Chooses and uses a generation method.
- [GenMethod](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Tile/GenMethods/GenMethod.cs): Defines abstraction for a generation method - all 3 phases.
- [Triangle](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Tile/GenMethods/Triangle.cs): Example implementation of a generation method.

The most natural starting point to me was generating the "tile map" that the game would take place on. The goal was to generate a hidden word somewhere in a sea of fake tiles. Early on I determined that process
should happen over 3 phases:

1. Create all tile objects in some shape pattern.
2. Generate the correct word's path.
3. Fill in all other fake tiles with convincing, but not unfair, fake letters.

None of these tasks are too hard individually. But combining them all, and doing so in a way that felt fair and fun, was a challenge. Here were some problems I ran into on the way:
- What shape?
  - If we used a simple rectangle, the player would have many options for their first letter. It made the game pretty difficult.
  - I found that triangles (pointed like '<') were easier. At the start, you'd only have one or two options for your first letter, and then it expanded from there. That gave you a good starting hint which you could build off of.
<img width="800" height="400" alt="image" src="https://github.com/user-attachments/assets/cb0db84f-2a7b-4b12-8125-4ea92b77acff" />

- Generating other shapes?
  - When the triangle got old, I wanted to be able to switch up shape generation on a whim.
  - I abstracted Tilemap generation so that I could use one of multiple algorithms for generating the shape (triangle, reversed triangle, rectangle, winding rectangle.)
  - I allowed overriding the other two phases as well, if needed - but it turned out they'd work with any shape, so I didn't have to.
  - I made the data structure that stores all tiles a Dictionary as opposed to a 2D array because I considered having some shapes not well suited for that, such as circles and branching paths.

- Backtracking?
  - Each hex tile technically has up to 6 hex adjacencies. But if we're always going forward, the player only has to care about 2.
  - I wanted the correct path to be able to wind to the side, or even backwards, allowing for more interesting paths the player had to really think about.
  - I accomplished this by generating a random number, used as the number of 'backtracks' in the correct path.
    - Going to the side costed 1 backtrack, going backwards costed 2.
   
- Shorter paths?
  - Implementing backtracks meant there were situations where the word path could wind back on itself. That meant you could reach the end goal without spelling out the entire word, or without spelling it in order.
  - I decided to reward the player for skipping letters and finding a shorter path. Usually it was because they knew the word and identified the shorter path. But there were also happy accidents, which I thought were fun!
 <img width="800" height="341" alt="image" src="https://github.com/user-attachments/assets/d880e6d2-cd77-4822-94e8-1a053427a3f1" />

- Convincing Fakes
  - What letters should we give the "fake" tiles? (Excluding letters that would cause contradictions- see below)
  - I debated 3 approaches: Totally random, Proportionally random (based on frequency of letters in English), and Markov (given previous letter, what is probability of each letter being the next?)
  - I went with proportionally random, finding the others made the game too easy and too hard, respectively. [LetterGen source code.](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Assignment/LetterGen.cs)

- Ensuring no contradictions
  - When generating "fake" letters, I had to make sure none of them would be letters that'd accidentally generate another path for the correct word. Returning to the previous example:
<img width="800" height="324" alt="image" src="https://github.com/user-attachments/assets/10ce24dd-4e21-4674-bb68-5cbf6c1b2485" />
<em>Fortunately this was simple. Just don't let any tiles bordering a correct tile have that correct tile's letter, nor its correct neighbors.</em>

- Alternative spellings of a word
  - Words like Skeptic (Sceptic) or Characterize (Characterise) have multiple correct English spellings. How would we deal with those?
  - I used ChatGPT to help identify alternative spellings of words - and if there were any, I included them with the word's definition (see below section on Word lists.)
  - In fake tile generation, if there are alternate spellings, I used a similar idea in ensuring fake tiles next to correct tiles could not potentially spell out the correct word in any of its forms.

### Gameplay
This phase of development started on paper, but ultimately took place through trial and error. I needed to figure out how to make the game playable, fun, and sufficiently challenging:
- I made my generation algorithms have configurable inputs that'd effect the size and difficulty of the resulting tilemap. I increase the difficulty associated with these inputs with each level.
- '[Items](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/ItemsScript.cs)' are special powerups that can give useful hints or bypass danger. They cost a 'totem', which doubles as an extra life.
  - I made totems serve both purposes for sheer simplicity. Thankfully, it worked well in practice and playtesting.
- The amount of levels beat, the amount of mistakes made, and total time taken all factor into a player's "high score" if they manage to win the game.
- '[Challenges](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Menu/ChallengePopup.cs)' are optional modifiers that make the game more difficult, such as cryptic special tiles, harder level generation, or a constantly ticking timer that gradually collapses rows.
  - If you beat a word list with all 5 challenges and make no mistakes, you get a "Gold Star" - the highest possible rank and the ultimate goal of Wordwalker.
    - As your reward, you unlock new characters with Gold Stars.
   
<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/0263e5fa-6c9d-481b-b55a-36997b7dc2a3" />\
<em>What the game looks like with all 5 challenges enabled. Note the special tiles, the timer, larger map, and gray totem (indicating the "Iron Man" challenge.) There's also a layer of fog that covers back rows.</em>

<img width="1083" height="507" alt="image" src="https://github.com/user-attachments/assets/3787c150-cb3a-4568-ae94-3aa442525627" />\
<em>The explanation popup for special tiles.</em>

<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/e53d6524-7c4e-44bf-8d24-67f8e8af1088" />\
<em>Postgame stats</em>


### Game State
The "state" in a game of Wordwalker entails a number of things. I split these things up across multiple manager files such as:
- [WalkManager](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Manager/WalkManager.cs): Stepping on tiles and controlling what the character is doing.
- [AnimationManager](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Manager/AnimationManager.cs): Character animations.
- [PlayerManager](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Manager/PlayerManager.cs): Camera movement.
- [WordwalkerUIManager](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Manager/WordwalkerUIScript.cs): Top-left UI such as score and rank.
- [GameManager](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Manager/GameManagerSc.cs): High-level management.
  - When we load into the "In game" scene (from the menu scene), we make sure all these managers have started before we do anything game-related.
  - When we restart a game there are various state variables that are automatically reset to defaults, by creating an entirely new state object.

### Word Lists
Next up: Getting words to use.

My original idea was to have 3 gigantic "easy / medium / hard" word lists, comprised of hundreds or thousands of words each, making it possible to play the game on an endless loop. I needed the sets to be large to avoid the so-called "Birthday Problem": In simple terms, it doesn't theoretically take long to run into duplicate words.

I realized a better approach was to instead have many smaller word lists, structured around various themes and categories. And, with respect to the Birthday Problem, I wrote code to "cycle" through the list of words, making sure each word was seen once before resetting the cycle (see the Persistent Storage section.) This made it easier to review each word, and give the player at least some idea of what they were getting into.

Design Choices:
- Structure of lists:
  - They are more or less CSV files, with a vertical bar ('|') as the separator since that character rarely appears in English text.
  - When looking for a random word from the list, you can simply get a random line in the file by choosing a random number. That line has the word, its clue, and other info you need.
  - [WordGen source code.](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Assignment/WordGen.cs)
- Image clues:
  - I realized I wanted to have word lists with image clues as well as textual clues. Structurally, word lists that use images are set up the same, except the "clue" is the filepath of the image to load.
- Obtaining the lists:
  - I got to show off some of my data science skills by obtaining the data in a variety of ways.
    - Factual word lists, such as "country capitals" or "periodic table elements" were easy enough to find online and convert to my file structure with regex expressions.
    - Several word lists, particularly ones with image clues, were obtained with scripting and data scraping. I wrote Python scripts to get images for IMDB's "top 100 actors" and all 150 Gen 1 Pokemon from the Pokemon Wiki.
    - Word lists of general vocabulary are among the ideal use cases for ChatGPT. Though I generally don't like using AI to make games, this was a situation that absolutely called for it. The clues and definitions it came up with were highly accurate and better than what I could do manually over countless more hours.
    - AI wasn't perfect, though. I wrote scripts to review each word list and find obvious problems: self-referencing definitions, re-used lemmas, and words that were too short or contained special characters, to name a few.
- Storage and Loading:
  - I decided to learn AssetBundles in the hopes of quickly loading this data only when I needed it, and I wasn't let down.
  - I'm able to load up each word list in full only when the player selects it.
    - Image clues are more specific, we only load each image when it is time to use its corresponding word.
- The Menu Screen:
  - I'm able to easily add or remove word lists by updating a single text file, [databases.txt](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Menu/databases.txt). The parsing and handling of the list itself is all done in [DatabaseParser](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Menu/DatabaseParser.cs).

I feel I did a good, efficient job with word lists - both in procuring them and using them in-game.

### UI Design
I gained a lot of appreciation for UI designers after having to try my own hand at it. The long story short is, it either looks good and is natural to understand, or it's not.

I do a lot of front-end development at my current job, particularly in Angular (HTML/JS), which gave me clarity in terms of setup strategy and what sort of components I could emulate. Such as:
- Widget Popup: A generic menu or infographic which slides onto and off of the screen with a smooth transition. You can only have one widget popup active at a time.
- Drop down menu: Similar to a "mat-accordion" in Angular, this component is used in the Free Play menu. It expands to display all databases in a particular category.

<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/a8a4b782-0c79-43c7-b661-a306f1368fb8" />\
<em>The settings menu implements WidgetPopup. It can be opened through a separate button and has the same Red X "close" button that other widgets do.</em>

<img width="337" height="248" alt="image" src="https://github.com/user-attachments/assets/f04e62be-68fd-4167-8cb6-7e1703693266" />
<em>The drop down at work in the Free Play menu.</em>

Wordwalker was designed for both iOS and Windows. One annoying feature about newer iPhone models, like my own, is this annoying divot at the top which cuts into the top of the screen. Unity has a "Screen.safeArea" field that can detect this, but it's up to the developer to use it. I wrote a script attached to almost all my UI elements called [ScalingUIComponent](https://github.com/alexman37/wordwalker/blob/main/Assets/Scripts/Wordwalker/Manager/UI/ScalingUIComponent.cs) that can automatically reposition and rescale a widget based on supplied proportions of the safeArea. It effectively relies on some basic assumptions - like the screen being wider than it is taller - but that hasn't been a problem yet.

For the overarching UI design of the two scenes, I made them with different goals in mind:
- Main Menu: Needed to be very easy to understand, and easy to launch into a game, while also having all important options a few obvious clicks away.
  - I'm especially happy with how the Free Play menu screen turned out. Here the player can see only the word lists they want to, and how well they've done on each.
- In-game: By comparison, the UI here was minimalist. The player only needed to know a few things: Score, rank, level number, lives remaining, and the current clue. All these were placed in obvious positions. There's also buttons for using power-ups and changing the camera mode, but you can easily ignore them if you choose.

<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/ed7212ae-a9b9-40e9-a373-a321d69fd255" />\
<em>The Main menu.</em>

<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/f100f70b-b003-4726-a508-6e04e9b269ae" />\
<em>Free Play menu. Note a database list turns to gold when the player earns a gold star on it. Throw 'em a bone every now and then.</em>

<img width="1366" height="768" alt="image" src="https://github.com/user-attachments/assets/78628b51-17d3-4241-a386-58dd252e1143" />\
<em>The in-game UI (for a picture clue). Note it's minimalist, and easy to ignore when you just want to focus on the word.</em>

<img width="402" height="262" alt="image" src="https://github.com/user-attachments/assets/53f6cc7a-87d2-4617-871d-8ec2a5bf6791" />
<em>The items submenu</em>


### Art
I'm no Van Gogh, but with Wordwalker I hope I was able to at least demonstrate a basic level of competence in the field. To run down a few topics quickly:
- Background art, character art and sprite sheets were all made in Paint.net, a simplistic drawing program (you can still do a lot with 'simple'. And it's free!) With the sprite sheets especially, I got pretty good with the program and found various tricks to speed the process up. I made all 9 spritesheets in 2 days.
- Animations were handled using a monstrosity of a state manager. Next time, I will probably get used to coding certain animations to play and not over-relying on transitions. This was something that snowballed out of hand quickly and one of the things I really feel I could do better on next time.
- The modeling and lighting for the temple exterior was done with Blender in a day. I wanted 3D models for the characters, but recognized my limitations and decided I wasn't there - yet. Lighting and shading are the biggest skills I want to practice on in future games.

<img width="868" height="481" alt="image" src="https://github.com/user-attachments/assets/e840927f-1220-492e-8e6f-3881e2cf1c89" />\
<em>8 of the playable characters in Wordwalker. Admittedly, some were borrowed from other game ideas of mine.</em>

<img width="420" height="360" alt="image" src="https://github.com/user-attachments/assets/30e4932f-8101-46a5-a7d1-f434b09c2dce" />\
<em>Character spritesheet. I got pretty good at making these quickly.</em>


### Persistent Storage
Persistent storage had two uses in Wordwalker:
- Global variables, such as your current preferred settings, your daily word streak, and what characters you have unlocked.
- Stats specific to each word list - your win/loss stats, your high scores, and the "word cycle": which words you have already seen (until it resets).
I deliberately chose not to encrypt this data. As a single player, unpaid, offline game not hooked up to any achievements services, I'm okay with lettings players muck around in the files if that is what they wish. It also means you get to see these files in action, if you're interested!

### Bugfixing, Playtesting, and Release
As early as May, I had August 1st as my target date for being completely finished the game. I was careful to focus on the skeleton and major features of the project first. Then, I polished up the look by focusing on art, UI and overall design. Lastly, I swept the whole project for bugs by my own testing, and most importantly- passing the game around to anyone who'd play it.

The bugfixing phase began in July. By then the game had become my entire focus in life, I was using basically every free moment I had to work on it or test it.

I won't claim it's "bug free" but I think I did everything I could to ensure the product that ultimately did release on August 1st was as smooth as possible.

### Conclusion
As of the time of writing this, Wordwalker has gone relatively under the radar (one thing I'm not is a marketer.) It has, however, received feedback from all the people in my life I've shared it with. And I feel it's genuine.

If you made it this far, thank you. I hope you got a chance to play it and enjoyed it. I hope this explanation serves as a demonstration of my technical abilities in various areas of game development.

I want to be a game developer more than anything. It's the only career I can see myself doing years from now. I want a chance to be creative every day, to work on projects I'm proud to share with the world.

I'm ready for all the struggles that entails. I've been through it before.
