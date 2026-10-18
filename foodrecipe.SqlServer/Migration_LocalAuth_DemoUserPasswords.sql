/*
    Demo/dev local-auth seed password
    ---------------------------------
    Shared password for the seeded AppUser records in this script:

        FoodRecipe123!

    This migration updates existing databases that were created before
    real PBKDF2 password hashes were added to the SQL seed data.
*/

UPDATE [dbo].[AppUser]
SET
    [PasswordHash] = CASE [UserName]
        WHEN 'chef_samia' THEN 0xD668DB854A70F8BD3DAF405020793D4473DEDE6F73A800847B706B556FBDB2BE
        WHEN 'chef_LOL' THEN 0x2CBB346C16B552B4E1F1FD8625BB69513BAA4624179A4555F00C8A80C1737039
        WHEN 'nora_gourmande' THEN 0x8FD9DE2B79768097B5FD85D79E835372F0B41675DB0711E9D5F0C7623755C154
        WHEN 'adam_cuisine' THEN 0x310571D0A48835F7FB88041DE5EA30E3CA9EF5300C64F13FD0B87542A02FC2CD
        WHEN 'sara_saveurs' THEN 0xDC36DD400A8463B5EBD7630614FC0F77F4C5D70A2C25FE6EA759E25D36CC015F
        ELSE [PasswordHash]
    END,
    [PasswordSalt] = CASE [UserName]
        WHEN 'chef_samia' THEN 0x62649150219B91828157808840623BAD
        WHEN 'chef_LOL' THEN 0xD30ECBF67A0A320A094852F7749AA8FB
        WHEN 'nora_gourmande' THEN 0x013A8D07CC91A531097A7491991F922C
        WHEN 'adam_cuisine' THEN 0x592A7D6B2BF28F9C5CF88A6F7592DEF4
        WHEN 'sara_saveurs' THEN 0x947E9956DBFF3EEB694461B3BBE84152
        ELSE [PasswordSalt]
    END
WHERE [UserName] IN (
    'chef_samia',
    'chef_LOL',
    'nora_gourmande',
    'adam_cuisine',
    'sara_saveurs'
);
