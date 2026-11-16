# PROJECT RULES — NT106 BÀI TẬP 3

## 1. Scope and authorization
- Complete all required assignment tasks from Task 1 through Task 9.
- Task 10 is explicitly excluded. Do not implement it.
- You are authorized to modify project source code as necessary to complete Tasks 1–9.
- Do not make unrelated changes, add unnecessary features, or redesign working parts without a clear technical reason.
- Do not change anything outside the project scope without asking for permission.
- If an important requirement is ambiguous, stop and ask before making a decision that could significantly affect the project.

## 2. Inspect before changing
- Read the entire existing solution before implementation.
- Inspect the project structure, source files, project references, configuration, and existing Git history.
- Review the assignment PDF and use it as the primary source of requirements.
- Preserve existing working functionality wherever possible.
- Do not blindly replace files or rewrite the entire solution.
- Identify dependencies and potential side effects before modifying code.

## 3. Permission and change control
- Never delete, rename, move, or overwrite existing files unless necessary for the authorized implementation.
- Never delete existing user data, databases, database tables, records, or Git history.
- Never execute destructive SQL statements.
- Never modify an existing database schema or execute database migrations without explicit permission.
- Never change project-wide settings, target frameworks, or major dependencies without a clear reason. Ask first if the change is significant.
- Never overwrite local changes that existed before your work.
- Never use force commands in Git.
- Never push, publish, deploy, or upload code without explicit permission.
- Do not modify files unrelated to Tasks 1–9.

## 4. Git and automatic commits
- Automatically commit your changes after each successfully completed implementation milestone.
- Before editing, inspect `git status` and record which changes already existed.
- Never include pre-existing user changes in your commits.
- Only stage files that you personally changed for the current milestone.
- Never use `git add .` or `git add -A` blindly.
- Use clear, descriptive commit messages.
- Suggested commit format:
  - `feat(shared): define TCP protocol`
  - `feat(server): implement concurrent TCP handling`
  - `feat(auth): implement registration and login`
  - `feat(client): integrate user information and logout`
  - `fix(ui): improve validation and connection status`
  - `test: add integration and error handling tests`
- Before each commit, inspect the staged diff.
- Never commit passwords, connection strings containing credentials, API keys, secrets, personal data, build artifacts, or temporary files.
- If a milestone cannot be verified, do not falsely mark it complete or commit it as successfully tested.
- Do not push commits to any remote repository.

## 5. Architecture and coding standards
- Preserve the separation between Client, Server, and SharedLibrary.
- The Client must communicate with the Server through TCP.
- The Client must not connect directly to SQL Server.
- All database operations must happen on the Server.
- Use asynchronous network and database operations where appropriate.
- Support multiple simultaneous TCP clients safely.
- Use proper resource disposal and cancellation handling.
- Use parameterized SQL queries.
- Never store plaintext passwords.
- Use the existing password hashing implementation where appropriate, improving it only when technically necessary.
- Keep protocol messages consistent between Client and Server.
- Handle TCP message framing correctly; do not assume one TCP read equals one complete message.
- Keep code readable, maintainable, and consistent with the existing project conventions.
- Avoid unnecessary dependencies.

## 6. Security and configuration
- Do not hardcode database credentials in source code.
- Do not ask the user to share database passwords in chat.
- Use an appropriate local configuration mechanism or environment variables for sensitive settings.
- Do not expose password hashes, salts, credentials, or internal exception details to the Client.
- Validate all input on the Server, even if the Client already validates it.
- Use safe error messages for users and detailed logs for developers.
- Do not log passwords or other sensitive authentication data.

## 7. Testing and verification
- Build the solution after meaningful implementation milestones.
- Run available tests and add tests where appropriate.
- Test registration, duplicate usernames, login success, login failure, user information, logout, multiple clients, and disconnection handling.
- Test invalid input, malformed messages, network failures, and database failures.
- Do not claim that something works unless it has actually been verified.
- Clearly distinguish implemented, built, tested, and runtime-verified features.
- If a required tool, database, or environment is unavailable, explain the limitation and provide the exact manual verification steps.

## 8. Communication and reporting
- Work through Tasks 1–9 systematically.
- Maintain a checklist showing completed, in-progress, blocked, and not-yet-started tasks.
- Explain important architectural decisions briefly.
- Before making a change that requires additional permission, stop and ask.
- Do not silently skip requirements.
- Do not hide build errors, test failures, or incomplete functionality.
- At the end, provide:
  1. A summary of implemented tasks.
  2. A list of modified and added files.
  3. Build and test results.
  4. Remaining issues and manual setup requirements.
  5. Git commits created.
  6. Instructions for running the Client and Server.

## 9. Final restriction
The objective is to complete Tasks 1–9 of the existing NT106 assignment, not to create a different project. Preserve the existing work, respect the assignment PDF, and request permission before any action outside the authorization above.