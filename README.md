# Tetris-AI

Unity 2023 - editor version 2023.1.0b11

Basic recreation of Tetris, with garbage system. Implemented AI is trained with a genetic learning algorithm that optimizes scoring algorithms to determine the optimal move-set.

### AI

The AI works off 8 individual scoring algorithms, each with a corresponding weight. The scores are:
1. Low Board
2. Air Pockets
3. Lines Cleared
4. Elevation Desparity
5. Minimize Wells
6. Tetris
7. Right Well
8. Blocks Above Air

Using the available hold piece and the currently held piece, the AI looks at all possible placements for the tetrominoes and evaluates the board substate with the scoring algorithms mentioned above. After determining the highest scoring board substate it uses the corresponding moveset that is generated alongside the board substate and sends it to the AI move handler.

##### Low Board

Low board represents the amount of fully clear lines from top to the first occupied line. Taking for example the attached image below: the height the highest piece reaches is line 8, therefore leaving 12 clear lines. This board gets a low board score of 12.

<img width="346" alt="LowBoard" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/06a6c324-4f3a-4369-ac8f-3ee6a4837fd5">

##### Air Pockets

Air pockets represents how many air tiles are covered by a roof. This evaluation returns negative since air pockets are intentionally avoided. Taking the example below, 2 air tiles have a solid tile (roof tile) above them, therefore this board gets an air pocket score of -2. 

<img width="346" alt="AirPockets" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/9b375a43-4b0c-4650-87b2-6f4df32e9961">

##### Lines Cleared

Lines cleared represents how many lines will be fully occupied, resulting in a line clear on that row. For the below board, 1 line is fully occupied, therefore that line will be cleared. This board gets a lines cleared score of 1.

<img width="346" alt="LinesCleared" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/9510a391-30d5-4f63-8f21-a5e8de820b96">

##### Elevation Disparity

Elevation disparity represents how "bumpy" the board is. To evaluate this, the elevation is evaluated at each column. Following this, the absolute value of the difference in adjacent elevations is added for each column. This sum is returned as a negative since elevation desparity is intentionally avoided. For example, on the board below, the elevation disparity across the board is 19 since moving from left to right, you would have to 'climb' up or down 19 tiles, therefore the board has an elevation disparity of 19.

<img width="346" alt="ElevationDisparity" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/6cac2b09-0bbe-4f05-a892-0e74fa4d2472">

##### Minimize Wells

Minimize wells represents the amount of holes with a depth of 3 or more. A hole depth is determined with the column elevation mentioned above. A hole's depth is determined as the minimum of the 2 adjacent wall height differences. For the below example, the left hole is considered a well since both sides are higher than 3. The right hole is not, since the left wall is only 2 high, therefore this board gets a minimize wells score of -1. 

<img width="346" alt="MinimizeWells" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/8cf348e3-ef19-4886-b246-a7de11aa1712">

##### Tetris

Tetris represents a 4-line clear. It is given a fixed return value determined purely by whether a tetris exists or not. If it does, 1 is awarded, else 0. For the below board, since a tetris is achieved via 4 cleared lines, this boards' tetris score is 1.

<img width="346" alt="Tetris" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/cc13d1eb-ade5-4e69-9286-b11bff7a7b41">

##### Right Well

Right well is implemented to promote a well on the side of the board. When tiles fill the right well, a negative score is given relative to the elevation of the right side of the board. For the board below, since the elevation of the board at the right side is 3, this board gets a right well score of -3.

<img width="346" alt="RightWell" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/cb02d298-eb3a-4e92-8d5b-262913d07b84">

##### Blocks Above Air

Blocks above air is similar to air pockets as it punishes gaps in the board. Blocks above air additionally punishes stacking blocks overtop of an air gap. This is determined by counting the amount of blocks above air tiles and returning the negative of that number. For the board below, since 5 solid blocks sit above air blocks, this board gets a blocks above air score of -5.

<img width="346" alt="BlocksAboveAir" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/9c318bd8-1be8-452a-a8dc-1c16e9f99082">

### Genetic Learning Algorithm

The genetic learning algorithm improves and optimizes the AI by running 100 iterations of the game at once and keeping the top 10% to repopulate the 100 iterations. The top 10% are selected by how long they survived, therefore the 10 who survived the longest move on to the next generation. Mutation is done by a mutation chance and a mutation strength. Each trait of the AI has a 20% chance to mutate and will mutate the trait by some multiplier between 0.5 and 2.0. Each row in the picture below represents the surviving mutant on the left, and 9 mutants to the right of it. Mutants have stronger mutations as they move to the right. To ensure a generation doesn't run too long, each iteration of the game is sent garbage at a fixed interval. The amount of garbage that is sent scales with runtime of the generation.

<img width="459" alt="GeneticLearning" src="https://github.com/williamt1117/Tetris-AI/assets/92940760/d4144eca-61fa-450f-ac18-56ff134f4fb1">
