---
description: Take a Jira ticket from SCRUM through the full band — branch, implement, gates, PR, ticket updated.
argument-hint: SCRUM-123
---

Take ticket **$1** through the pipeline. Jira project `SCRUM`, cloud id `eeb14bc5-0c0e-4814-a5ff-a36d6bc0a41b`.

Work in this order and do not skip a step because it looks obvious.

**1. Read the ticket, and read it as a contract.** Fetch $1 and its parent. Extract the Gate — the explicit acceptance criterion. If the ticket has no checkable Gate, write one into the ticket before touching code; a task whose completion cannot be verified will be argued about later.

**2. Refuse work that is not ready.** If the inputs are ambiguous, the design is missing, or the Gate cannot be made concrete, say so and stop. A ticket bounced early is cheap; a wrong implementation is not.

**3. Branch.** `feature/jira-$1`, cut from an up-to-date `origin/main`. Move the ticket to **В работе**.

**4. Delegate to the specialist who owns those files.** Entities to `domain-coder`, interactors and ports to `usecase-coder`, tests to `test-engineer`, NPC decisions to `npc-behaviour`, data to `data-smith`, dialogue to `narrative-writer`, Unity to `unity-adapter`, scenes to `scene-composer`, light to `lighting-artist`. Two agents must never be given the same file. If the work spans bands, run them in sequence and hand over explicitly.

**5. Run the gates yourself.** `dotnet build` and `dotnet test Game.sln --configuration Release`, then the dependency audit over `src/Game.Domain` and `src/Game.Application`. Quote the real counts — only numbers you actually saw. Then hand the diff to `qa-reviewer` and act on what comes back.

**6. Commit by explicit path.** Never `git add -A`: `MYgame/` is a 2.9 GB Unity project sitting in this working tree. The hooks will stop you, but do not rely on them.

**7. Open the PR** against `main`, titled `$1: <what changed>`, with the gate results and their evidence in the body. Move the ticket to **В процессе проверки** and comment the PR link on it.

**8. Stop there.** Merging into `main` is the Principal's decision (CLAUDE.md section 5). Report what is ready and wait. Once Yura authorizes and the merge lands, move the ticket to **Готово** with the final SHA and the test output.
