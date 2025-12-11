-- SQL Seed Data Script for BoardGameNight Database
-- This script populates the database with test data for demo purposes
-- It is idempotent - can be run multiple times safely

-- Clear existing test data (optional - comment out if you want to preserve existing data)
-- Note: This will delete ALL data, not just test data
-- DELETE FROM GameRatings;
-- DELETE FROM BoardGameConversationMessages;
-- DELETE FROM BoardGameConversations;
-- DELETE FROM BoardGameFaqCaches;
-- DELETE FROM GameRegistrations;
-- DELETE FROM Registrations;
-- DELETE FROM BoardGameMetadata;
-- DELETE FROM BoardGameCaches;
-- DELETE FROM BoardGames;
-- DELETE FROM Sessions;

-- ============================================================================
-- 1. USERS (stored as UserId strings in Registrations and GameRatings)
-- ============================================================================
-- Note: Users are represented by UserId strings. We'll use these in Registrations and GameRatings.
-- User IDs (for reference):
-- 'user-001-alice-smith'
-- 'user-002-bob-jones'
-- 'user-003-charlie-brown'
-- 'user-004-diana-prince'
-- 'user-005-eve-wilson'
-- 'user-006-frank-miller'
-- 'user-007-grace-taylor'

-- ============================================================================
-- 2. BOARD GAMES
-- ============================================================================
-- Deck-Building Games
INSERT OR REPLACE INTO BoardGames (Id, Name, Description, SetupComplexity, Score, AveragePlaytimeMinutes, CreatedAt, LastUpdatedAt) VALUES
('11111111-1111-1111-1111-111111111101', 'Dominion', 'A deck-building card game where players build their deck during gameplay.', 2.5, 4.2, 30, datetime('now'), datetime('now')),
('11111111-1111-1111-1111-111111111102', 'Star Realms', 'A fast-paced deck-building game of space combat.', 2.0, 4.0, 20, datetime('now'), datetime('now')),
('11111111-1111-1111-1111-111111111103', 'Clank! A Deck-Building Adventure', 'A deck-building adventure game with dungeon exploration.', 2.5, 4.3, 60, datetime('now'), datetime('now')),
('11111111-1111-1111-1111-111111111104', 'The Quest for El Dorado', 'A deck-building race game where players explore to find El Dorado.', 2.0, 4.1, 45, datetime('now'), datetime('now')),
('11111111-1111-1111-1111-111111111105', 'Dune: Imperium', 'A deck-building worker placement game set in the Dune universe.', 3.5, 4.5, 120, datetime('now'), datetime('now')),
('11111111-1111-1111-1111-111111111106', '51st State (Ultimate Edition)', 'A post-apocalyptic engine-building and deck-building game.', 3.0, 4.2, 90, datetime('now'), datetime('now')),
('11111111-1111-1111-1111-111111111107', 'Legendary: A Marvel Deck Building Game', 'A cooperative deck-building game featuring Marvel superheroes.', 2.5, 4.0, 45, datetime('now'), datetime('now')),
('11111111-1111-1111-1111-111111111108', 'Aeon''s End', 'A cooperative deck-building game where players defend the last city.', 3.0, 4.4, 60, datetime('now'), datetime('now'));

-- Classic & Deep Economic Strategy Games
INSERT OR REPLACE INTO BoardGames (Id, Name, Description, SetupComplexity, Score, AveragePlaytimeMinutes, CreatedAt, LastUpdatedAt) VALUES
('22222222-2222-2222-2222-222222222201', 'Brass: Birmingham', 'An economic strategy game set during the Industrial Revolution in Birmingham.', 4.0, 4.6, 120, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222202', 'Power Grid', 'A strategy game about powering cities with different types of power plants.', 3.5, 4.3, 120, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222203', 'Puerto Rico', 'A classic economic strategy game of building and trading.', 3.5, 4.4, 90, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222204', 'Settlers of Catan', 'A classic resource management and trading game.', 2.5, 4.2, 60, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222205', 'Le Havre', 'A strategic game about building a shipping empire in Le Havre.', 4.0, 4.5, 150, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222206', 'A Feast for Odin', 'A worker placement game about Viking life and exploration.', 4.0, 4.5, 120, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222207', 'Keyflower', 'A strategic game combining worker placement and tile placement.', 3.5, 4.3, 90, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222208', 'Great Western Trail', 'A strategic game about cattle ranching and delivering to Kansas City.', 3.5, 4.4, 120, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222209', 'Food Chain Magnate', 'A strategic game about building a fast-food chain empire.', 4.5, 4.6, 180, datetime('now'), datetime('now')),
('22222222-2222-2222-2222-222222222210', 'Isle of Skye', 'A tile-laying and auction game set in Scotland.', 2.5, 4.1, 60, datetime('now'), datetime('now'));

-- ============================================================================
-- 3. SESSIONS
-- ============================================================================
-- Historical Sessions (past dates)
INSERT OR REPLACE INTO Sessions (Id, Date, CreatedAt, IsCancelled) VALUES
('33333333-3333-3333-3333-333333333301', date('now', '-90 days'), datetime('now', '-90 days'), 0),
('33333333-3333-3333-3333-333333333302', date('now', '-60 days'), datetime('now', '-60 days'), 0),
('33333333-3333-3333-3333-333333333303', date('now', '-30 days'), datetime('now', '-30 days'), 0),
('33333333-3333-3333-3333-333333333304', date('now', '-14 days'), datetime('now', '-14 days'), 0);

-- Upcoming Sessions (future dates)
INSERT OR REPLACE INTO Sessions (Id, Date, CreatedAt, IsCancelled) VALUES
('44444444-4444-4444-4444-444444444401', date('now', '+7 days'), datetime('now'), 0),
('44444444-4444-4444-4444-444444444402', date('now', '+21 days'), datetime('now'), 0),
('44444444-4444-4444-4444-444444444403', date('now', '+35 days'), datetime('now'), 0);

-- ============================================================================
-- 4. REGISTRATIONS (Users registering for sessions)
-- ============================================================================
-- Historical Session 1 (90 days ago)
INSERT OR REPLACE INTO Registrations (Id, UserId, UserDisplayName, FoodRequirements, CreatedAt, SessionId) VALUES
('55555555-5555-5555-5555-555555555501', 'user-001-alice-smith', 'Alice Smith', 'Vegetarian', datetime('now', '-90 days'), '33333333-3333-3333-3333-333333333301'),
('55555555-5555-5555-5555-555555555502', 'user-002-bob-jones', 'Bob Jones', NULL, datetime('now', '-90 days'), '33333333-3333-3333-3333-333333333301'),
('55555555-5555-5555-5555-555555555503', 'user-003-charlie-brown', 'Charlie Brown', 'Gluten-free', datetime('now', '-90 days'), '33333333-3333-3333-3333-333333333301'),
('55555555-5555-5555-5555-555555555504', 'user-004-diana-prince', 'Diana Prince', NULL, datetime('now', '-90 days'), '33333333-3333-3333-3333-333333333301');

-- Historical Session 2 (60 days ago)
INSERT OR REPLACE INTO Registrations (Id, UserId, UserDisplayName, FoodRequirements, CreatedAt, SessionId) VALUES
('55555555-5555-5555-5555-555555555505', 'user-001-alice-smith', 'Alice Smith', 'Vegetarian', datetime('now', '-60 days'), '33333333-3333-3333-3333-333333333302'),
('55555555-5555-5555-5555-555555555506', 'user-002-bob-jones', 'Bob Jones', NULL, datetime('now', '-60 days'), '33333333-3333-3333-3333-333333333302'),
('55555555-5555-5555-5555-555555555507', 'user-005-eve-wilson', 'Eve Wilson', 'Vegan', datetime('now', '-60 days'), '33333333-3333-3333-3333-333333333302'),
('55555555-5555-5555-5555-555555555508', 'user-006-frank-miller', 'Frank Miller', NULL, datetime('now', '-60 days'), '33333333-3333-3333-3333-333333333302');

-- Historical Session 3 (30 days ago)
INSERT OR REPLACE INTO Registrations (Id, UserId, UserDisplayName, FoodRequirements, CreatedAt, SessionId) VALUES
('55555555-5555-5555-5555-555555555509', 'user-003-charlie-brown', 'Charlie Brown', 'Gluten-free', datetime('now', '-30 days'), '33333333-3333-3333-3333-333333333303'),
('55555555-5555-5555-5555-555555555510', 'user-004-diana-prince', 'Diana Prince', NULL, datetime('now', '-30 days'), '33333333-3333-3333-3333-333333333303'),
('55555555-5555-5555-5555-555555555511', 'user-005-eve-wilson', 'Eve Wilson', 'Vegan', datetime('now', '-30 days'), '33333333-3333-3333-3333-333333333303'),
('55555555-5555-5555-5555-555555555512', 'user-007-grace-taylor', 'Grace Taylor', NULL, datetime('now', '-30 days'), '33333333-3333-3333-3333-333333333303');

-- Historical Session 4 (14 days ago)
INSERT OR REPLACE INTO Registrations (Id, UserId, UserDisplayName, FoodRequirements, CreatedAt, SessionId) VALUES
('55555555-5555-5555-5555-555555555513', 'user-001-alice-smith', 'Alice Smith', 'Vegetarian', datetime('now', '-14 days'), '33333333-3333-3333-3333-333333333304'),
('55555555-5555-5555-5555-555555555514', 'user-002-bob-jones', 'Bob Jones', NULL, datetime('now', '-14 days'), '33333333-3333-3333-3333-333333333304'),
('55555555-5555-5555-5555-555555555515', 'user-006-frank-miller', 'Frank Miller', NULL, datetime('now', '-14 days'), '33333333-3333-3333-3333-333333333304'),
('55555555-5555-5555-5555-555555555516', 'user-007-grace-taylor', 'Grace Taylor', NULL, datetime('now', '-14 days'), '33333333-3333-3333-3333-333333333304');

-- Upcoming Session 1 (7 days from now)
INSERT OR REPLACE INTO Registrations (Id, UserId, UserDisplayName, FoodRequirements, CreatedAt, SessionId) VALUES
('66666666-6666-6666-6666-666666666601', 'user-001-alice-smith', 'Alice Smith', 'Vegetarian', datetime('now'), '44444444-4444-4444-4444-444444444401'),
('66666666-6666-6666-6666-666666666602', 'user-003-charlie-brown', 'Charlie Brown', 'Gluten-free', datetime('now'), '44444444-4444-4444-4444-444444444401'),
('66666666-6666-6666-6666-666666666603', 'user-005-eve-wilson', 'Eve Wilson', 'Vegan', datetime('now'), '44444444-4444-4444-4444-444444444401');

-- Upcoming Session 2 (21 days from now)
INSERT OR REPLACE INTO Registrations (Id, UserId, UserDisplayName, FoodRequirements, CreatedAt, SessionId) VALUES
('66666666-6666-6666-6666-666666666604', 'user-002-bob-jones', 'Bob Jones', NULL, datetime('now'), '44444444-4444-4444-4444-444444444402'),
('66666666-6666-6666-6666-666666666605', 'user-004-diana-prince', 'Diana Prince', NULL, datetime('now'), '44444444-4444-4444-4444-444444444402'),
('66666666-6666-6666-6666-666666666606', 'user-006-frank-miller', 'Frank Miller', NULL, datetime('now'), '44444444-4444-4444-4444-444444444402');

-- Upcoming Session 3 (35 days from now)
INSERT OR REPLACE INTO Registrations (Id, UserId, UserDisplayName, FoodRequirements, CreatedAt, SessionId) VALUES
('66666666-6666-6666-6666-666666666607', 'user-001-alice-smith', 'Alice Smith', 'Vegetarian', datetime('now'), '44444444-4444-4444-4444-444444444403'),
('66666666-6666-6666-6666-666666666608', 'user-003-charlie-brown', 'Charlie Brown', 'Gluten-free', datetime('now'), '44444444-4444-4444-4444-444444444403'),
('66666666-6666-6666-6666-666666666609', 'user-007-grace-taylor', 'Grace Taylor', NULL, datetime('now'), '44444444-4444-4444-4444-444444444403');

-- ============================================================================
-- 5. GAME REGISTRATIONS (Games brought to sessions)
-- ============================================================================
-- Historical Session 1 games
INSERT OR REPLACE INTO GameRegistrations (Id, RegistrationId, BoardGameId) VALUES
('77777777-7777-7777-7777-777777777701', '55555555-5555-5555-5555-555555555501', '11111111-1111-1111-1111-111111111101'), -- Alice: Dominion
('77777777-7777-7777-7777-777777777702', '55555555-5555-5555-5555-555555555501', '11111111-1111-1111-1111-111111111102'), -- Alice: Star Realms
('77777777-7777-7777-7777-777777777703', '55555555-5555-5555-5555-555555555502', '22222222-2222-2222-2222-222222222204'), -- Bob: Settlers of Catan
('77777777-7777-7777-7777-777777777704', '55555555-5555-5555-5555-555555555503', '11111111-1111-1111-1111-111111111103'), -- Charlie: Clank!
('77777777-7777-7777-7777-777777777705', '55555555-5555-5555-5555-555555555504', '22222222-2222-2222-2222-222222222203'); -- Diana: Puerto Rico

-- Historical Session 2 games
INSERT OR REPLACE INTO GameRegistrations (Id, RegistrationId, BoardGameId) VALUES
('77777777-7777-7777-7777-777777777706', '55555555-5555-5555-5555-555555555505', '11111111-1111-1111-1111-111111111105'), -- Alice: Dune: Imperium
('77777777-7777-7777-7777-777777777707', '55555555-5555-5555-5555-555555555506', '22222222-2222-2222-2222-222222222201'), -- Bob: Brass: Birmingham
('77777777-7777-7777-7777-777777777708', '55555555-5555-5555-5555-555555555507', '11111111-1111-1111-1111-111111111108'), -- Eve: Aeon's End
('77777777-7777-7777-7777-777777777709', '55555555-5555-5555-5555-555555555508', '22222222-2222-2222-2222-222222222202'); -- Frank: Power Grid

-- Historical Session 3 games
INSERT OR REPLACE INTO GameRegistrations (Id, RegistrationId, BoardGameId) VALUES
('77777777-7777-7777-7777-777777777710', '55555555-5555-5555-5555-555555555509', '11111111-1111-1111-1111-111111111104'), -- Charlie: The Quest for El Dorado
('77777777-7777-7777-7777-777777777711', '55555555-5555-5555-5555-555555555510', '22222222-2222-2222-2222-222222222205'), -- Diana: Le Havre
('77777777-7777-7777-7777-777777777712', '55555555-5555-5555-5555-555555555511', '11111111-1111-1111-1111-111111111106'), -- Eve: 51st State
('77777777-7777-7777-7777-777777777713', '55555555-5555-5555-5555-555555555512', '22222222-2222-2222-2222-222222222206'); -- Grace: A Feast for Odin

-- Historical Session 4 games
INSERT OR REPLACE INTO GameRegistrations (Id, RegistrationId, BoardGameId) VALUES
('77777777-7777-7777-7777-777777777714', '55555555-5555-5555-5555-555555555513', '11111111-1111-1111-1111-111111111107'), -- Alice: Legendary: Marvel
('77777777-7777-7777-7777-777777777715', '55555555-5555-5555-5555-555555555514', '22222222-2222-2222-2222-222222222207'), -- Bob: Keyflower
('77777777-7777-7777-7777-777777777716', '55555555-5555-5555-5555-555555555515', '22222222-2222-2222-2222-222222222208'), -- Frank: Great Western Trail
('77777777-7777-7777-7777-777777777717', '55555555-5555-5555-5555-555555555516', '22222222-2222-2222-2222-222222222209'); -- Grace: Food Chain Magnate

-- Upcoming Session 1 games
INSERT OR REPLACE INTO GameRegistrations (Id, RegistrationId, BoardGameId) VALUES
('88888888-8888-8888-8888-888888888801', '66666666-6666-6666-6666-666666666601', '22222222-2222-2222-2222-222222222210'), -- Alice: Isle of Skye
('88888888-8888-8888-8888-888888888802', '66666666-6666-6666-6666-666666666602', '11111111-1111-1111-1111-111111111101'), -- Charlie: Dominion
('88888888-8888-8888-8888-888888888803', '66666666-6666-6666-6666-666666666603', '11111111-1111-1111-1111-111111111103'); -- Eve: Clank!

-- Upcoming Session 2 games
INSERT OR REPLACE INTO GameRegistrations (Id, RegistrationId, BoardGameId) VALUES
('88888888-8888-8888-8888-888888888804', '66666666-6666-6666-6666-666666666604', '22222222-2222-2222-2222-222222222201'), -- Bob: Brass: Birmingham
('88888888-8888-8888-8888-888888888805', '66666666-6666-6666-6666-666666666605', '22222222-2222-2222-2222-222222222204'), -- Diana: Settlers of Catan
('88888888-8888-8888-8888-888888888806', '66666666-6666-6666-6666-666666666606', '11111111-1111-1111-1111-111111111105'); -- Frank: Dune: Imperium

-- Upcoming Session 3 games
INSERT OR REPLACE INTO GameRegistrations (Id, RegistrationId, BoardGameId) VALUES
('88888888-8888-8888-8888-888888888807', '66666666-6666-6666-6666-666666666607', '22222222-2222-2222-2222-222222222202'), -- Alice: Power Grid
('88888888-8888-8888-8888-888888888808', '66666666-6666-6666-6666-666666666608', '11111111-1111-1111-1111-111111111102'), -- Charlie: Star Realms
('88888888-8888-8888-8888-888888888809', '66666666-6666-6666-6666-666666666609', '22222222-2222-2222-2222-222222222203'); -- Grace: Puerto Rico

-- ============================================================================
-- 6. GAME RATINGS (Ratings for historical sessions only)
-- ============================================================================
-- Historical Session 1 ratings
INSERT OR REPLACE INTO GameRatings (Id, UserId, BoardGameId, SessionId, Rating, CreatedAt, UpdatedAt) VALUES
('99999999-9999-9999-9999-999999999901', 'user-001-alice-smith', '11111111-1111-1111-1111-111111111101', '33333333-3333-3333-3333-333333333301', 5, datetime('now', '-89 days'), datetime('now', '-89 days')), -- Alice rates Dominion: 5
('99999999-9999-9999-9999-999999999902', 'user-001-alice-smith', '11111111-1111-1111-1111-111111111102', '33333333-3333-3333-3333-333333333301', 4, datetime('now', '-89 days'), datetime('now', '-89 days')), -- Alice rates Star Realms: 4
('99999999-9999-9999-9999-999999999903', 'user-002-bob-jones', '22222222-2222-2222-2222-222222222204', '33333333-3333-3333-3333-333333333301', 5, datetime('now', '-89 days'), datetime('now', '-89 days')), -- Bob rates Settlers of Catan: 5
('99999999-9999-9999-9999-999999999904', 'user-003-charlie-brown', '11111111-1111-1111-1111-111111111103', '33333333-3333-3333-3333-333333333301', 4, datetime('now', '-89 days'), datetime('now', '-89 days')), -- Charlie rates Clank!: 4
('99999999-9999-9999-9999-999999999905', 'user-004-diana-prince', '22222222-2222-2222-2222-222222222203', '33333333-3333-3333-3333-333333333301', 5, datetime('now', '-89 days'), datetime('now', '-89 days')), -- Diana rates Puerto Rico: 5
('99999999-9999-9999-9999-999999999906', 'user-002-bob-jones', '11111111-1111-1111-1111-111111111101', '33333333-3333-3333-3333-333333333301', 3, datetime('now', '-89 days'), datetime('now', '-89 days')), -- Bob rates Dominion: 3
('99999999-9999-9999-9999-999999999907', 'user-003-charlie-brown', '22222222-2222-2222-2222-222222222204', '33333333-3333-3333-3333-333333333301', 2, datetime('now', '-89 days'), datetime('now', '-89 days')); -- Charlie rates Settlers of Catan: 2

-- Historical Session 2 ratings
INSERT OR REPLACE INTO GameRatings (Id, UserId, BoardGameId, SessionId, Rating, CreatedAt, UpdatedAt) VALUES
('99999999-9999-9999-9999-999999999908', 'user-001-alice-smith', '11111111-1111-1111-1111-111111111105', '33333333-3333-3333-3333-333333333302', 5, datetime('now', '-59 days'), datetime('now', '-59 days')), -- Alice rates Dune: Imperium: 5
('99999999-9999-9999-9999-999999999909', 'user-002-bob-jones', '22222222-2222-2222-2222-222222222201', '33333333-3333-3333-3333-333333333302', 5, datetime('now', '-59 days'), datetime('now', '-59 days')), -- Bob rates Brass: Birmingham: 5
('99999999-9999-9999-9999-999999999910', 'user-005-eve-wilson', '11111111-1111-1111-1111-111111111108', '33333333-3333-3333-3333-333333333302', 4, datetime('now', '-59 days'), datetime('now', '-59 days')), -- Eve rates Aeon's End: 4
('99999999-9999-9999-9999-999999999911', 'user-006-frank-miller', '22222222-2222-2222-2222-222222222202', '33333333-3333-3333-3333-333333333302', 4, datetime('now', '-59 days'), datetime('now', '-59 days')), -- Frank rates Power Grid: 4
('99999999-9999-9999-9999-999999999912', 'user-001-alice-smith', '22222222-2222-2222-2222-222222222201', '33333333-3333-3333-3333-333333333302', 3, datetime('now', '-59 days'), datetime('now', '-59 days')), -- Alice rates Brass: Birmingham: 3
('99999999-9999-9999-9999-999999999913', 'user-005-eve-wilson', '11111111-1111-1111-1111-111111111105', '33333333-3333-3333-3333-333333333302', 2, datetime('now', '-59 days'), datetime('now', '-59 days')); -- Eve rates Dune: Imperium: 2

-- Historical Session 3 ratings
INSERT OR REPLACE INTO GameRatings (Id, UserId, BoardGameId, SessionId, Rating, CreatedAt, UpdatedAt) VALUES
('99999999-9999-9999-9999-999999999914', 'user-003-charlie-brown', '11111111-1111-1111-1111-111111111104', '33333333-3333-3333-3333-333333333303', 4, datetime('now', '-29 days'), datetime('now', '-29 days')), -- Charlie rates The Quest for El Dorado: 4
('99999999-9999-9999-9999-999999999915', 'user-004-diana-prince', '22222222-2222-2222-2222-222222222205', '33333333-3333-3333-3333-333333333303', 5, datetime('now', '-29 days'), datetime('now', '-29 days')), -- Diana rates Le Havre: 5
('99999999-9999-9999-9999-999999999916', 'user-005-eve-wilson', '11111111-1111-1111-1111-111111111106', '33333333-3333-3333-3333-333333333303', 3, datetime('now', '-29 days'), datetime('now', '-29 days')), -- Eve rates 51st State: 3
('99999999-9999-9999-9999-999999999917', 'user-007-grace-taylor', '22222222-2222-2222-2222-222222222206', '33333333-3333-3333-3333-333333333303', 5, datetime('now', '-29 days'), datetime('now', '-29 days')), -- Grace rates A Feast for Odin: 5
('99999999-9999-9999-9999-999999999918', 'user-003-charlie-brown', '22222222-2222-2222-2222-222222222205', '33333333-3333-3333-3333-333333333303', 1, datetime('now', '-29 days'), datetime('now', '-29 days')), -- Charlie rates Le Havre: 1
('99999999-9999-9999-9999-999999999919', 'user-005-eve-wilson', '22222222-2222-2222-2222-222222222206', '33333333-3333-3333-3333-333333333303', 4, datetime('now', '-29 days'), datetime('now', '-29 days')); -- Eve rates A Feast for Odin: 4

-- Historical Session 4 ratings
INSERT OR REPLACE INTO GameRatings (Id, UserId, BoardGameId, SessionId, Rating, CreatedAt, UpdatedAt) VALUES
('99999999-9999-9999-9999-999999999920', 'user-001-alice-smith', '11111111-1111-1111-1111-111111111107', '33333333-3333-3333-3333-333333333304', 4, datetime('now', '-13 days'), datetime('now', '-13 days')), -- Alice rates Legendary: Marvel: 4
('99999999-9999-9999-9999-999999999921', 'user-002-bob-jones', '22222222-2222-2222-2222-222222222207', '33333333-3333-3333-3333-333333333304', 4, datetime('now', '-13 days'), datetime('now', '-13 days')), -- Bob rates Keyflower: 4
('99999999-9999-9999-9999-999999999922', 'user-006-frank-miller', '22222222-2222-2222-2222-222222222208', '33333333-3333-3333-3333-333333333304', 5, datetime('now', '-13 days'), datetime('now', '-13 days')), -- Frank rates Great Western Trail: 5
('99999999-9999-9999-9999-999999999923', 'user-007-grace-taylor', '22222222-2222-2222-2222-222222222209', '33333333-3333-3333-3333-333333333304', 5, datetime('now', '-13 days'), datetime('now', '-13 days')), -- Grace rates Food Chain Magnate: 5
('99999999-9999-9999-9999-999999999924', 'user-001-alice-smith', '22222222-2222-2222-2222-222222222207', '33333333-3333-3333-3333-333333333304', 3, datetime('now', '-13 days'), datetime('now', '-13 days')), -- Alice rates Keyflower: 3
('99999999-9999-9999-9999-999999999925', 'user-002-bob-jones', '22222222-2222-2222-2222-222222222208', '33333333-3333-3333-3333-333333333304', 4, datetime('now', '-13 days'), datetime('now', '-13 days')), -- Bob rates Great Western Trail: 4
('99999999-9999-9999-9999-999999999926', 'user-006-frank-miller', '22222222-2222-2222-2222-222222222209', '33333333-3333-3333-3333-333333333304', 2, datetime('now', '-13 days'), datetime('now', '-13 days')); -- Frank rates Food Chain Magnate: 2

