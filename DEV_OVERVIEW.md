## Wordwalker - From Start to Finish
In this writeup, I'll explain the entire process of how I developed Wordwalker over the course of 5 months - everything from "Hello World" to releasing the final build! It's going to be a long one, so skip around to wherever interests you most, if you like.

#### Background and Concept
The idea for Wordwalker randomly came to be in November of 2024 when I was taking a shower. I thought of a scene from Indiana Jones 3, when Indy is solving a puzzle in a temple. I have absolutely no idea why I randomly remembered this.

The clip starts at 1:40:
https://www.youtube.com/watch?v=XqGWI0WTj24

<img width="1009" height="440" alt="image" src="https://github.com/user-attachments/assets/6d0ab255-8dd7-42ae-94f9-c3d72f41f4e8" />
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

<img width="1384" height="1038" alt="image" src="https://github.com/user-attachments/assets/3c1b12ae-0ec9-4edf-a527-2d962eca4b41" />
<em>There's about 2 times more pages than what you see.</em>

#### Tilemap Generation
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

- Ensuring no contradictions
  - When generating "fake" letters, I had to make sure none of them would be letters that'd accidentally generate another path for the correct word. Returning to the previous example:
<img width="800" height="324" alt="image" src="https://github.com/user-attachments/assets/10ce24dd-4e21-4674-bb68-5cbf6c1b2485" />
<em>Fortunately this was simple. Just don't let any tiles bordering a correct tile have that correct tile's letter, nor its correct neighbors.</em>

- Alternative spellings of a word
  - Words like Skeptic (Sceptic) or Characterize (Characterise) have multiple correct English spellings. How would we deal with those?
  - I used ChatGPT to help identify alternative spellings of words - and if there were any, I included them with the word's definition (see below section on Word lists.)
  - In fake tile generation, if there are alternate spellings, I used a similar idea in ensuring fake tiles next to correct tiles could not potentially spell out the correct word in any of its forms.


#### Game State
The "state" in a round of Wordwalker entails a number of things. I split these things up across multiple manager files such as:
- WalkManager: Stepping on tiles and controlling what the character is doing.
- AnimationManager: Character animations.
- PlayerManager: Camera movement.
- WordwalkerUIManager: Top-left UI such as score and rank.
- GameManager: High-level management.
  - When we load into the "In game" scene (from the menu scene), we make sure all these managers have started before we do anything game-related.
  - When we restart a game there are various state variables that are automatically reset to defaults.

#### Word Lists
Next up: Getting words to use.

My original idea was to have 3 gigantic "easy / medium / hard" word lists, comprised of hundreds or thousands of words each, making it possible to play the game on an endless loop. I needed the sets to be large to avoid the so-called "Birthday Problem": In simple terms, it doesn't theoretically take long to run into duplicate words.

I realized a better approach was to instead have many smaller word lists, structured around various themes and categories. And, with respect to the Birthday Problem, I wrote code to "cycle" through the list of words, making sure each word was seen once before resetting the cycle (see the Persistent Storage section.) This made it easier to review each word, and give the player at least some idea of what they were getting into.

Design Choices:
- Structure of lists:
  - They are more or less CSV files, with a vertical bar ('|') as the separator since that character rarely appears in English text.
  - When looking for a random word from the list, you can simply get a random line in the file by choosing a random number. That line has the word, its clue, and other info you need.
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
  - I'm able to easily add or remove word lists by updating a single text file, databases.txt. The parsing and handling of the list itself is all done elsewhere.

I feel I did a good, efficient job with word lists - both in procuring them and using them in-game.

