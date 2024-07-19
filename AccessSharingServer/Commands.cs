using System.Drawing;

namespace AccessSharingServer
{
    public class Commands
    {
        static bool isEnabled = false;
        static string[]? commandWords;
        static List<Command> commands = new List<Command>();

        public static void Enable()
        {
            isEnabled = true;
            Initialize();
            Loop();
        }

        static void Loop()
        {
            while (isEnabled)
            {
                string? command = Console.ReadLine();
                if (command != null)
                {
                    commandWords = command.Split(' ');
                    CheckCommand();

                }
            }
        }

        static void CheckCommand()
        {
            foreach (Command command in commands)
            {
                if (command.Name == commandWords[0])
                {
                    command.Action(commandWords);
                    return;
                }
            }
            Console.WriteLine($"Unknown command! Type 'help' for list of commands.");
        }

        static void Initialize()
        {
            commands.Clear();

            commands.Add(new Command()
            {
                Name = "help",
                Action = new Action<string[]>((commandWords) =>
                {
                    Console.WriteLine($"List of commands: ");
                    foreach (Command command in commands)
                    {
                        Console.WriteLine($"{command.Name}");
                    }
                    return;
                })
            });

            commands.Add(new Command()
            {
                Name = "stop",
                Action = new Action<string[]>((commandWords) =>
                {
                    Console.WriteLine("Closing server...");
                    Program.Shutdown();
                    return;
                })
            });

            commands.Add(new Command()
            {
                Name = "save",
                Action = new Action<string[]>((commandWords) =>
                {
                    Console.WriteLine("Saving...");
                    Program.Save();
                    Console.WriteLine("Saved!");
                    return;
                })
            });

            commands.Add(new Command()
            {
                Name = "role",
                Action = new Action<string[]>((commandWords) =>
                {
                    if(commandWords.Length < 3)
                    {
                        Console.WriteLine($"role (name) (create, remove, permission, accessLevel, dataTag, color)");
                        return;
                    }
                    if (RoleLibrary.GetRole(commandWords[1]) == null && commandWords[2] != "create")
                    {
                        Console.WriteLine($"role {commandWords[1]} not found");
                        return;
                    }
                    if (commandWords[2] == "create")
                    {
                        RoleLibrary.CreateRole(new Role()
                        {
                            Name = commandWords[1],
                        });
                        Console.WriteLine($"Role {commandWords[1]} created!");
                        return;
                    }
                    if (commandWords[2] == "remove")
                    {
                        RoleLibrary.RemoveRole(commandWords[1]);
                        Console.WriteLine($"Role {commandWords[1]} removed!");
                        return;
                    }
                    if (commandWords[2] == "permission")
                    {
                        if (commandWords.Length < 5)
                        {
                            Console.WriteLine($"role (name) permission (add, remove) (permissionIndex)");
                            Console.WriteLine($"{commandWords[1]} permissions:");
                            foreach (Permission p in RoleLibrary.GetRole(commandWords[1]).Permissions)
                            {
                                Console.WriteLine($"{p} ({(int)p})");
                            }
                            return;
                        }
                        if (commandWords[3] == "add")
                        {
                            if(commandWords[4] == "*")
                            {
                                for(int i = 0; i < (int)Permission.MaxValue; i++)
                                {
                                    RoleLibrary.GetRole(commandWords[1]).AddPermission((Permission)i);
                                }
                                Console.WriteLine($"Added all permissions to role {commandWords[1]}");
                                return;
                            }
                            try
                            {
                                RoleLibrary.GetRole(commandWords[1]).AddPermission((Permission)Convert.ToInt32(commandWords[4]));
                                Console.WriteLine($"Added permission {(Permission)Convert.ToInt32(commandWords[4])} to role {commandWords[1]}");
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }
                        if (commandWords[3] == "remove")
                        {
                            if (commandWords[4] == "*")
                            {
                                for (int i = 0; i < (int)Permission.MaxValue; i++)
                                {
                                    RoleLibrary.GetRole(commandWords[1]).RemovePermission((Permission)i);
                                }
                                Console.WriteLine($"Removed all permissions from role {commandWords[1]}");
                                return;
                            }
                            try
                            {
                                RoleLibrary.GetRole(commandWords[1]).RemovePermission((Permission)Convert.ToInt32(commandWords[4]));
                                Console.WriteLine($"Removed permission {(Permission)Convert.ToInt32(commandWords[4])} from role {commandWords[1]}");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }
                    }
                    if (commandWords[2] == "accessLevel")
                    {
                        if (commandWords.Length < 4)
                        {
                            Console.WriteLine($"role (name) accessLevel (accessLevel)");
                            Console.WriteLine($"{commandWords[1]} access level is {RoleLibrary.GetRole(commandWords[1]).AccessLevel}:");
                            return;
                        }
                        try
                        {
                            RoleLibrary.GetRole(commandWords[1]).SetAccessLevel(Convert.ToInt32(commandWords[3]));
                            Console.WriteLine($"{commandWords[1]}'s access level is now {Convert.ToInt32(commandWords[3])}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return;
                        }
                    }
                    if (commandWords[2] == "dataTag")
                    {
                        if (commandWords.Length < 5)
                        {
                            Console.WriteLine($"role (name) dataTag (add, remove) (dataTag)");
                            Console.WriteLine($"{commandWords[1]} data tags:");
                            foreach (string s in RoleLibrary.GetRole(commandWords[1]).DataTags)
                            {
                                Console.WriteLine(s);
                            }
                            return;
                        }
                        if (commandWords[3] == "add")
                        {
                            try
                            {
                                RoleLibrary.GetRole(commandWords[1]).AddDataTag(commandWords[4]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }
                        if (commandWords[3] == "remove")
                        {
                            try
                            {
                                RoleLibrary.GetRole(commandWords[1]).RemoveDataTag(commandWords[4]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }
                    }
                    if (commandWords[2] == "color")
                    {
                        if (commandWords.Length < 6)
                        {
                            Console.WriteLine($"role (name) color (R) (G) (B)");
                            Color c = Color.FromArgb(RoleLibrary.GetRole(commandWords[1]).Color);
                            Console.WriteLine($"{commandWords[1]} color is {c.R} {c.G} {c.B}");
                            return;
                        }
                        try
                        {
                            RoleLibrary.GetRole(commandWords[1]).Color = Color.FromArgb(255, Convert.ToByte(commandWords[3]), Convert.ToInt32(commandWords[4]), Convert.ToInt32(commandWords[5])).ToArgb();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return;
                        }
                    }
                })
            });

            commands.Add(new Command()
            {
                Name = "user",
                Action = new Action<string[]>((commandWords) =>
                {
                    if (commandWords.Length < 3)
                    {
                        Console.WriteLine($"user (login) (role, name)");
                        return;
                    }
                    if (AccountManager.GetAccount(commandWords[1]) == null)
                    {
                        Console.WriteLine($"user {commandWords[1]} not found");
                        return;
                    }
                    if (commandWords[2] == "role")
                    {
                        if (commandWords.Length < 5)
                        {
                            Console.WriteLine($"user (login) role (add, remove) (roleName)");
                            Console.WriteLine($"{commandWords[1]} roles:");
                            foreach (string s in AccountManager.GetAccount(commandWords[1]).RoleManager.Roles)
                            {
                                Console.WriteLine($"{s}");
                            }
                            return;
                        }
                        if (commandWords[3] == "add")
                        {
                            try
                            {
                                AccountManager.GetAccount(commandWords[1]).RoleManager.AddRole(commandWords[4]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }
                        if (commandWords[3] == "remove")
                        {
                            try
                            {
                                AccountManager.GetAccount(commandWords[1]).RoleManager.RemoveRole(commandWords[4]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                return;
                            }
                        }
                    }
                    if (commandWords[2] == "name")
                    {
                        if (commandWords.Length < 4)
                        {
                            Console.WriteLine($"user (login) name (name)");
                            Console.WriteLine($"{commandWords[1]} name: {AccountManager.GetAccount(commandWords[1]).Name}");
                            return;
                        }
                        AccountManager.GetAccount(commandWords[1]).Name = commandWords[3];
                        return;
                    }
                })
            });

            commands.Add(new Command()
            {
                Name = "accountList",
                Action = new Action<string[]>((commandWords) =>
                {
                    List<Account> accounts = AccountManager.GetAccounts();
                    Console.WriteLine("Accounts:");
                    foreach (Account account in accounts)
                    {
                        Console.WriteLine($"- Name: {account.Name} | Login: {account.Login}");
                    }
                })
            });

            commands.Add(new Command()
            {
                Name = "permissionList",
                Action = new Action<string[]>((commandWords) =>
                {
                    Console.WriteLine("Permissions:");
                    for (int i = 0; i < (int)Permission.MaxValue; i++)
                    {
                        Console.WriteLine($"- {(Permission)i} ({i})");
                    }
                })
            });

            commands.Add(new Command()
            {
                Name = "roleList",
                Action = new Action<string[]>((commandWords) =>
                {
                    Console.WriteLine("Roles:");
                    foreach(Role r in RoleLibrary.GetRolesListSorted())
                    {
                        Console.WriteLine($"- {r.Name} | AccessLevel {r.AccessLevel}");
                    }
                })
            });
        }
    }
}
