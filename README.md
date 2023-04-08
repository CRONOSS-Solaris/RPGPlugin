# RPG Plugin - Information for the tester.

## 1. Plugin Objective 

The RPG plugin introduces a system of roles, levels and experience for players in the game. Players can choose one of the available roles, gain experience, and advance in levels.*

## 2. Available functions and commands

- **!r setrole <roleName>:** Sets the selected role for the player.
- **!r roles:** Displays a list of available roles and their descriptions.
- **!r stats:** Displays the player's current level and the amount of experience needed to reach the next level.

## 3. Expected plugin behaviour

- The plugin should correctly change a player's role after using the **setrole** command.
- The plugin should display a list of available roles and their descriptions after using the **roles** command.
- The plugin should display the player's current level and the amount of experience needed to reach the next level after using the **stats** command.

## 4. Test scenarios

- **Normal use cases:** Check that all commands work correctly for typical parameter values.
- **Edge cases:** Try to use commands with incorrect parameters, such as a non-existent role.
- **Error cases:** Try to use commands without the required parameters or call commands in the wrong order.

## 5. How to report errors and suggestions

Please report any errors, comments or suggestions you encounter to the plugin author, e.g. via the discord server: 
<a href="https://discord.gg/AVQ347un9x">
    <img src="https://discordapp.com/api/guilds/1008603013025374208/widget.png?style=banner2" alt="Discord Server">
  </a>

## 6. Roles that are not implemented:

- Hunter
- Warrior

## Additional important information

It is recommended that the tester have some experience with the game, mods, plug-ins and the general structure of the game to more easily understand the plugin and its functions.
