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
