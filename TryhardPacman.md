# Orbital-Tryhard Pacman

**Proposed level of submission: Apollo 11**
Completed by: Malcolm Ong & Lim Jun Cheng

# 1. Motivation

Want to **have fun and relieve stress** with your friends? Play our modified version of PacMan and **relive your childhood memories!**

At the same time, to make PacMan **more engaging**, we have decided to augment the game with a pop quiz as an additional element of **surprise and competition**.

For players who think that PacMan is too simple of a game, we have included altered special components to the game to make it extra **challenging** at higher levels.

To account for the ever-changing COVID-19 situation, we have decided to scrap the idea of local multiplayer for our PacMan game and instead focus on supporting **online multiplayer** so that you can play this game with your friends from the comfort of your own home!

Our aim is to bring about a fresh twist to a beloved classic and more importantly, we want our users to have fun and enjoy themselves with their friends whilst playing our game.

# 2. User stories

Priority Legend:

*: low (possible extension)

**: medium (feature)

***: high (core feature)

| Priority | As a...  |I want... |So I can...|
|:--|:--:|:--:|:--:|
|** | new user |to know how the game works and how the game should be played | enjoy playing the game smoothly without any unnecessary disruptions |
|***| user | to select the appropriate game difficulty (easy, medium, hard) | progress and improve my skills step-by-step |
|***| user | to select my own pop quiz topics | utilise my specific trivia expertise |
|**| user | to create an account | store my personal collectibles and points gained from playing |
|**| administrator | to be able to access/delete an account | assist the user by fixing bugs in point/inventory recording or password resets |


# 3. Milestone 2 Feedback

 - We have added a pause/exit menu for single player mode as per one team evaluator’s request. No pausing for multiplayer since you cannot pause for such games like League of Legends and PUBG.
 - We have forced some of our friends to play and test the game for us and documented their thoughts and feedback in Section 6 below. Please check it out!
 - The shop and inventory system was created to allow players to get points and exchange them for stuff. The feedback given by the team evaluators was considered and now you can change the colour of the map and your Pacman character.
 - We tried to include more technical details and the full tech stack that we used for the project but this resulted in a Great Wall of Text and we did our best to streamline it with some diagrams/pictures.



# 4. Design


## 4.1. Architecture

The **Architecture Diagram** given above explains the high-level design of the App. Given below is a quick overview of each component._

The 3 main components of the game consist of:

- **UI:** this is used to display information to the user/player
- **Logic:** this acts as both a data bridge and controller to allow the game to function properly
- **Model:** this holds the database that stores the users’ information

Here we use a **_sequence diagram_** to show how the components interact with one another in the scenario where a user logs into the game:


## 4.2. UI Component


The **UI component** works with the logic component to retrieve relevant information from the model component to display in the game UI for a proper game experience.

It consists mainly of text, canvas, image and button components attached to multiple scenes in the game on Unity to work in tandem with various C# Scripts to display information on modified or retrieved data from the database. Further details on UI in Section 5.2.

## 4.3. Logic Component

The **logic component** acts as an essential data bridge between the model and UI component of the game as well as executing the main functions that allow the game to run smoothly. Some major examples of GameObjects that execute such functions are:

- **GameManager**: plays background music, facilitates changes in UI and player animation (for single player)
- **MainMenu**: holds the logic for room joining and matchmaking, is instantiated in the GameLobby (for multiplayer)
- **PlayerSpawner:** allows player(s) to be instantiated on the scene in the correct orientation and position
- **AccountManager**: has code that retrieves and saves information from the database, makes account creation possible

## 4.4. Model Component

The **model component** consists of a Realtime Database hosted by Google Firebase for this project. The transactions that take place between the logic and model components are as follows:

- Get/Set username and point information of player (AccountManager Script)
- Get/Set inventory and shop information of player (ItemManager Script)

# 5. Implementation & Features

We are proud to announce that we have completed our 2 main new features, the **online multiplayer battle royale** and **account creation** system! Below are the full details for all our features and the tech stack:

## 5.1. Tech Stack

- **Unity**, the main application we used to create the vast amounts of GameObjects, Scenes as well as C# Scripts to build our game from scratch. The true MVP.
- **Photon Unity Networking (PUN)** was the Unity Asset Store package that single-handedly handled the networking and Real Time syncing of events during multiplayer gameplay, hence making the multiplayer game possible to build in spite of the deprecation of the UNet.
- **Google FireBase** is a mobile and web application development platform that we used primarily for its authentication and database hosting services which made the account creation and inventory storage features possible.
- **GIMP** is an open-source graphics editor that we used to create all the sprites for the pacman game itself.
- **Audacity** is an open-source digital audio recording software and editor which we utilised to bring about most of the in-game sound effects.

## 5.2. Single Player Game:

### 1. Game Setup
- Designed the pellets and original game board level using GIMP software
- Reused the pellets to act as vertices for pacman to move from point to point (graph representation only at turning points)
- **Important:** For movements on the map without pellets, we utilised “fake pellets” such that allow pacman to move on them but appear invisible to the user
- Gateways are implemented with the same idea of the “fake pellets”, allowing pacman to teleport from one side to another magically
- Scatter spots are corner spots outside of the game board which will be used during scatter mode (explained later)

### 2. Pacman
- Designed pacman itself as a sprite using GIMP software
- Create a pacman script with important details such as its speed, orientation and direction for movement control
- Movement is formulated based on the vertex pacman is most recently on and the user input helps to determine changes in direction
- Rotation of pacman is adjusted using transform.localRotation and Quaternion.Euler (which is set according to the direction pacman is facing)

### 3. Different ghost AIs

- Formulated ghosts movements to be based on vertex movement as well BUT they follow a certain movement pattern respectively
- Create an enum class for the four different ghosts type in the ghost script: blinky, pinky, inky and clyde for convenient access
- Instead of movement controlled based on player input, they chase pacman with their specific movement pattern by the main driver method: MoveAndChasePacman()
- The end vertex of this method is adjusted according to the ghost type:
	- **Blinky** (the red ghost) will directly track and chase pacman’s current map position
	- **Pinky** (the pink ghost) will target 4 spaces ahead of pacman’s current map position
	- **Inky** (the cyan ghost) will try to corner blinky with its “random” movement pattern, targeting a tile by taking 2 spaces ahead of pacman and doubling the distance Blinky is away from it
	- **Clyde** (the orange ghost) moves according to how far away he is from pacman’s current map position. When he is more than 8 tiles away from pacman, his movements are identical to Blinky. When within 8 tiles from pacman, he will attempt to escape to its scatter position and “hide” there

### 4. Different modes to support actual gameplay

- To facilitate gameplay, four modes are distinguished similar to the actual game:
	- Chase mode: The ghosts will move based on their specific movement patterns and “hunt down” pacman
	- Scatter mode: The end vertex for each ghost will be their scatter spots at four corners of the map (to give the pacman player some breathing space)
	- Frightened mode: Activated upon the consuming of the power pellet, whereby the script allows for collision detection between pacman and the ghosts during this time period
		- Ghosts move much slower and turn blue during frightened mode
		- End vertex for each ghost become random
	- Consumed mode: Upon collision, ghosts move back rapidly to the ghost house via the shortest path possible

### 5. Scoring system

- Pellet: 10 * player level
- Power pellet: 100 * player level
- Consumed ghost: 200 (multiplied by 2 for additional ghost consumed during “same frightened mode)
- Bonus tile: 500
- Reversed mode: Score multiplier based on mode duration
- Key things:
	-	Points are scaled for users at higher levels accordingly
	-	High scores are updated live using playerprefs
	-	An option to reset high score is included under options

### 6. Difficulty Level

- 3 modes: easy, medium, difficult (for single player only)
- Increasing difficulty for players upon completion of a level:
	-	Shorter scatter time
	-	Shorter frightened duration
	-	Quicker ghost release duration
	-	Faster ghost speed to intensify the game
	-	Shorter powerup duration

### 7. Different stages/terrains

Used GIMP and unity’s tilemap to create 4 different maps other than the original one

### 8. Level progression/Gameover

- Game manager script:
	-	Checks for total pellets consumed to determine if the level is completed
	-	When level is completed, load next level using PhotonNetwork
	-	Upon collision detection, all sprites on the map are frozen and level is restarted
	-	When all lives are used up,
		-	Playerprefs for pacman and the map are resetted while high score is updated
		-	Leave the room using PhotonNetwork while main menu is loaded using the SceneManager in unity

### 9. Powerups

- Powerups are designed using GIMP and spawned randomly using the random generator by the UnityEngine:
	- Reverse mode: Movement are reversed for the pacman player but it includes a huge score multiplier
	- Frozen mode: All ghosts are frozen for a specific amount of time according to the game difficulty
	- Sped up mode: Pacman’s speed is doubled
	- (Bonus) An extra life is given when the pacman image is eaten
	- (Bonus) Extra points are obtainable (cherry replacement)

### 10. Pop Quiz Panel

- Upon losing a life or completion of a level, the pop quiz panel is set active
- PopQuizManager script is used to generate random questions from a pool of questions based on the 7 different categories provided
- Getting the question correct will grant the user an extra random powerup for the game

### 11. Pause Menu Panel

- Pressing the esc button allows the user to pause the game and take a break
- One can quit the game from the pause menu or return to main menu accordingly as requested by players who played the game

## 5.3. Multiplayer Game:
### 1. Photon plugin incorporation

Using the Photon Unity Networking Package available on the Unity Asset Store, we were able to set up a room joining system to allow for matchmaking during multiplayer games. Some important features are as follows:

**Matchmaking:**
- GameLobby Scene was created to allow players to pick the number of players they want to be matched up with as well as host players joining rooms; in single player mode it also acts as a level select panel
- **JoinRandomRoom() method:** as the name suggests it allows for quick matchmaking by allowing users to join random rooms
- **OnJoinedRoom() method:** after the player joins the room, he/she is assigned an ID number based on their order of entry into the room. For example, the 2nd player to enter the room gets an ID number of 2.
- **OnPlayerEnteredRoom() method:** this method is only important when the correct number of players have entered the room; now the master client will load the multiplayer level and the game will begin!

**Spawning + In-Game:**
- MultiPlayer Level Scene holds important GameObjects like GameManager (similar to the single player one in its function) and the PlayerSpawner, and also holds all the PacMan Players that will be spawned
- **PhotonNetwork.Instantiate():** using this method, PlayerSpawner will spawn the players in their respective positions on the map based on their ID number upon room entry previously. There are a maximum of 4 players on the field at any time
- **OnTriggerEnter2D() and PunRPC:** the former is used to detect collisions between PacMan players or collisions between players and objects to allow players to pick up items and/or lose lives to other players. The latter is then used to send such collision responses from the client(s) to the server in order to sync these collisions across all other player screens and allow for smooth gameplay
- **PhotonView:** observing the Photon Transform View and Photon Animation View of individual players allows their movements and animation to be synced with all other player screens

**Post-Game:**

- **LeaveRoom() method:** once a player dies, he/she will be presented with the option to quit the current game by pressing the “Exit Game” Button that pops up, or to continue spectating the game until it ends
- The sole survivor of the game will be greeted with a Congratulations screen and can then exit the game and start a new match

### 2. Resources/Prefabs for spawning

- Purpose of the Resources Folder: all power ups and Pacman prefabs are stored inside the Resources folder of the game as a requirement for the PhotonNetwork to spawn them via PhotonNetwork.Instantiate()
- MultiPower Script: this script is attached to all the powerup pickups in the game, mainly to call the self-destruction method on itself when consumed since only the master client or the owner of a PhotonView can destroy itself
- MultiGameManager Script: like single player mode it controls the music and various UI in the game, but it is also responsible for spawning the powerups periodically on 10 random locations on the game map, and also destroys them after a short period of time if not picked up
- Power Up Display UI for individual players: each player has their own UI to track the state of their picked up power ups such as number of remaining bullets and shield remaining time

## 5.4. Account Creation:

We used Google FireBase to implement an authentication system and a realtime database, which allowed for features such as a personal inventory system and storing of points.

### 1. Authentication System

- **RegisterAndSaveData() method:** this method calls FirebaseAuth.auth.CreateUserWithEmailAndPasswordAsync() and afterwards WriteNewUser() method (custom method for Realtime Database) which creates an account using the user’s email and a strong enough password
- **LogIn() method:** uses email and password to log-in to the game and calls LoadName() and LoadPoints() method
- **LogInGuest() method:** creates an anonymous “guest” account which does not store any data
- **LogOut() method:** logs the player out of the game
- **ChangePassword() method:** allows player to change password and also updates this to the database

### 2. Realtime Database

- The **Start() method** will set the editor database URL to the game database on Firebase to sync real-time updates and data retrieval when information is needed
- **WriteNewUser() method:** creates a new user with points , email and username information stored under the Users branch in the database
- **WriteNewItemStorage() method:** creates a new inventory boolean array for the user under the Items branch in the database
- **LoadPoints() and LoadName() method:** retrieves point and username data from the database to update the game UI for the current player
- **ChangeUsername() method:** changes username for player and updates in the database

### 3. Inventory System

- **ItemManager Script:** acts as a data bridge between the Realtime Database and the local game UI to coordinate data about bought items or points spent
- **SaveTransactions() method:** called when player leaves the shop; uploads the updated inventory boolean array to the database if any changes were made
- **UpdateUI() method:** updates the local game UI using database information
- Various buy methods: used as **OnClick() methods** for buttons, toggled whenever items are purchased to update the inventory array
- **UpdateInventory() method:** updates the boolean array from the database to the local game if any items are bought from the store, to prevent repeat purchasing and to record purchases

## 6. Testing

We have made the game available to some people in order to get them to test our game and give us some feedback. The following are the feedback that we gathered after their thorough playing and testing:

|Comments|Changes  |
|:--:|:--:|
| Initially, pacman were of the same color in multiplayers and users find it difficult to distinguish them | Created different color pacman for different users using additional separate prefabs |
| Users wanted to pause/return to main menu during the game to do other testing | Added a pause menu as per requested (only in single player mode) |
| User can move at quadruple the speed during multiplayer mode | Intended as feature |
| User can shoot the bullet at itself when moving at a speed faster than the bullet | Intended as feature, to balance out the destructive power of the player’s intense speed |
| Users wanted to be able to hide their password when entering it in the input fields | Added now after a hasty fix |

