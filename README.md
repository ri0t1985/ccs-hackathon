# Board Game Registration Form

A web application for registering board games that you plan to bring to events. This application allows users to sign in with their Microsoft account and register the board games they will bring.

## Features

### Board Game Registration Form

The **Register Game** page (`/register-game`) provides a simple and intuitive form for users to register their board games:

#### Form Fields

1. **Your Name** (Required)
   - Enter your full name or display name
   - This helps identify who is bringing the game

2. **Board Game Name** (Required)
   - Enter the name of the board game you plan to bring
   - Examples: "Settlers of Catan", "Ticket to Ride", "Wingspan", etc.

#### How to Use

1. **Sign In**: Navigate to the Login page and sign in with your Microsoft account
2. **Access Registration**: Once authenticated, go to the Register Game page from the navigation menu
3. **Fill Out Form**: 
   - Enter your name in the "Your Name" field
   - Enter the board game name in the "Board Game Name" field
4. **Submit**: Click the "Register" button to submit your registration
5. **Confirmation**: You'll see a confirmation message once your game is successfully registered

#### Form Validation

- Both fields are required and must be filled out before submission
- The form provides real-time validation feedback
- Clear error messages guide users if any required fields are missing

#### User Experience

- Clean, minimal design that focuses on ease of use
- Responsive layout that works on desktop and mobile devices
- Secure authentication ensures only authorized users can register games
- Simple confirmation message provides immediate feedback

## Technical Details

### Authentication

The application uses Microsoft Entra ID (Azure AD) for secure authentication. Users must sign in with their Microsoft account before accessing the registration form.

### Data Storage

Currently, the registration form displays a confirmation message. In a production environment, this would typically save the registration data to a database for event planning and tracking purposes.

### Access

- **URL**: `/register-game`
- **Authentication Required**: Yes
- **Access**: Available to all authenticated users

## Getting Started

1. Ensure you have the application running (see main project README for setup instructions)
2. Sign in with your Microsoft account
3. Navigate to the Register Game page
4. Fill out and submit the form

## Future Enhancements

Potential improvements to the board game registration form:

- View all registered games
- Edit or delete your registrations
- Add multiple games at once
- Include game details (number of players, play time, complexity)
- Export registered games list
- Integration with event management systems

