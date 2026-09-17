# Coin Rush

Coin Rush is a small 2D online platformer for two players. Both players move through the level, race to the current coin, and try to collect the highest score before the match timer ends.

This project was built as a gameplay and multiplayer prototype. The level uses simple placeholder visuals so the focus stays on movement, networking, and the match flow.

## Requirements

- Unity `6000.0.77f1`
- Android Build Support if you want to make an APK
- Two game instances for a multiplayer test
- A working Photon Fusion configuration and internet connection

## Running the project

1. Open the project with Unity `6000.0.77f1`.
2. Open the gameplay scene from `Assets/Scenes`.
3. Check the scene references before pressing Play:
   - `GameManager` has a player prefab and two spawn points.
   - `CoinManager` has the coin object and coin spawn points.
   - `UIManager` has all screens and the alert object assigned.
   - The player prefab has its `NetworkObject`, `Rigidbody2D`, colliders, and input references configured.
4. Enter a player name on the profile screen.
5. Create a room from one game instance and join it using the generated room code from the second instance.

The project currently uses Fusion sessions with a maximum of two players. For a local multiplayer test, ParrelSync or two built players can be used, provided both instances can reach the same Fusion session.

## Match flow

The normal flow is:

1. A player creates a room or joins an existing room with a room code.
2. The host and client are spawned into the same Fusion session.
3. The matchmaking screen waits until both players are present.
4. Both players are marked ready and the game countdown starts.
5. The host starts the 60-second match timer and spawns the first coin.
6. Players move and jump through the level to collect coins.
7. Every collected coin is replaced at another configured spawn point.
8. When the timer expires, the higher score wins.
9. If the opponent leaves during the match, the remaining player is shown as the winner.

## Architecture

The code is split into a few small responsibilities:

- `RoomManager` creates and shuts down the Fusion runner, creates or joins rooms, spawns players, and handles player connection events.
- `NetworkPlayerData` stores networked player information such as name, position, spawn point, and score. It also handles falling-player respawn.
- `PlayerController` reads local movement and jump input and applies it on the state-authoritative player object.
- `NetworkGameTimer` controls readiness, the match timer, match completion, and winner selection.
- `CoinManager` controls the single coin in the level and chooses the next spawn point.
- `PlayerManager` keeps direct references to the local player and opponent so gameplay and UI code do not repeatedly search the scene.
- `UIManager` switches between screens and exposes the reusable alert system.
- `Alert` is the shared Yes/No or OK popup used for connection errors and leave confirmation.
- `PlayerDataManager` saves the local player name using Unity `PlayerPrefs`.

## Networking and authority

Photon Fusion is used for the multiplayer layer.

The state authority is responsible for the game decisions that must be consistent for both players:

- Starting the match timer
- Spawning and moving the active coin
- Accepting coin collection
- Increasing the collecting player's score
- Ending the match
- Selecting the winner

Coin collection is handled by `Coin.OnTriggerEnter2D`, but the score is only changed when the object has state authority. This means two players cannot both receive a point for the same coin during a simultaneous collision. The coin's active spawn index and visibility are networked so both clients see the same coin.

Player input is gathered from the player with input authority and sent through Fusion. The authoritative player object applies the movement, while the other client receives the synchronized position.

## Level setup

The level is a hand-authored 2D platforming layout with platforms at different heights, side boundaries, a bottom area, and several possible routes toward the coin.

The level depends on these Inspector settings:

- Two safe player spawn points on `GameManager`
- Multiple reachable coin spawn points on `CoinManager`
- Platform `Collider2D` components
- A player fall limit below the lowest playable platform
- Player `Rigidbody2D` and `Collider2D` components
- A trigger `Collider2D` on the coin

Coin spawn points are predefined in the scene rather than generated at runtime. They should be placed above reachable platforms and away from walls or solid geometry.

## Controls

The game uses Unity's Input System.

- Move: horizontal input action or on-screen joystick
- Jump: jump input action or on-screen jump button
- Leave match: leave button, followed by the confirmation alert

Only the local player's input is read for that client, so the two players do not control each other.

## UI

The game includes screens for:

- Splash
- Profile
- Home
- Matchmaking
- Gameplay
- Results

During gameplay, the UI displays both player scores and the remaining time. The result screen shows the winner, draw state, or opponent-disconnected result together with the final scores.

## Bonus features

Along with the core requirements, the project includes these extra features:

- A reusable alert system with OK and Yes/No actions.
- A Leave Match feature during gameplay with Yes/No confirmation.
- A Home button on the result screen, allowing the player to return to the home screen and start another match.
- A simple room-join error popup for invalid room codes and connection failure reasons.
