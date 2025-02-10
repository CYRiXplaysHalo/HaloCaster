# Halo CE Caster

This repo is very much a WIP but wanted to share the current state of the code with some people so they could check it out.

## Ideas 

- Create scoreboard overlay
  - Show each team's stats
  - Show each player's individual stats
  - Track series score
    - Can you combinatin of boxes and player names to determine when to reset
- Create live minimap
  - Track the realtime location of every player
- Generate POV name overlays
  - Instead of having to manually type in each player's name, just use their in-game name.
  - We know console names, and players are indexed, so that should be enough.
  - Player index should be in order they are in-game, but need to confirm
- Generate in-game event feed overlay
  - Similar to a sports play-by-play, a feed that updates primarily with who killed who
  - Can get more complex things like which player picked up a powerup, a power weapon etc
- Generate advaned postgame carnage report
  - How often did you give your teammate a random?
  - How many powerups and power weapons did you acquire?
  - Maybe everything in Halo 2 PGCR? Things like accuracy, medals, dmg vs/against, etc
- HUD Message Injection
  - Look into inject messages into the HUD such as in a training mode let a player know whether or not they are standing on a random
  - Better facilitate online tunnel play by alerting players whenever a 4th player is ready to join while the other 3 warm up
