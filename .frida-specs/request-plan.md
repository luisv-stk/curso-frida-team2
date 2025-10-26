<tasks>
  <task>
    <task_name>Improve LLM API User Prompt</task_name>
    <subtasks>
      <subtask>
        <id>1</id>
        <name>Draft detailed user instruction</name>
        <description>Create a more specific, detailed user prompt that guides the LLM to generate comprehensive and precise image tags by specifying tag categories like objects, attributes, scene context, and stylistic elements, and instruct the model to output only a comma-separated list of tags without extra text.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>2</id>
        <name>Update prompt text in controller code</name>
        <description>Modify the LlmContentItem of type "text" in ImageTaggingController.cs to use the new detailed user prompt content.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>5</id>
        <name>Review and refine user prompt for clarity and specificity</name>
        <description>Critically assess the current user prompt content, optimize language for clarity and conciseness, ensure it covers all necessary tag categories, and update the LlmContentItem in ImageTaggingController.cs if improvements are identified.</description>
        <completed>true</completed>
      </subtask>
    </subtasks>
  </task>
  <task>
    <task_name>Add System Prompt for Tag Extraction</task_name>
    <subtasks>
      <subtask>
        <id>3</id>
        <name>Define system message for tag extraction</name>
        <description>Compose a system prompt message instructing the model to focus exclusively on extracting relevant image tags and to operate in tagging mode, ensuring it behaves as a tag extractor.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>4</id>
        <name>Integrate system message into request payload</name>
        <description>Add a LlmRequestMessage with role "system" and the defined system prompt content to the Messages array in ImageTaggingController.cs before the user message.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>6</id>
        <name>Review and refine system prompt for focus and precision</name>
        <description>Evaluate the existing system prompt for conciseness and effectiveness, adjust wording to reinforce the model's tagging role, and update the LlmRequestMessage in ImageTaggingController.cs if necessary.</description>
        <completed>true</completed>
      </subtask>
    </subtasks>
  </task>
</tasks>