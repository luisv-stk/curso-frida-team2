<tasks>
  <task>
    <task_name>Image Tagging Controller</task_name>
    <subtasks>
      <subtask>
        <id>1</id>
        <name>Design controller interface and route</name>
        <description>Create a new controller class within FridaApi, define a POST endpoint that accepts image uploads as input.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>2</id>
        <name>Implement image processing and base64 conversion</name>
        <description>Handle the incoming image file in the endpoint, convert the image data to a Base64-encoded string.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>3</id>
        <name>Integrate external LLM API call</name>
        <description>Configure an HTTP client to send a POST request to https://frida-llm-api.azurewebsites.net/v1/chat/completions with the Base64 image payload and the required Bearer token.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>4</id>
        <name>Parse and return image tags</name>
        <description>Process the response from the LLM API to extract suggested tags and return them as the endpoint response.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>5</id>
        <name>Add endpoint tests</name>
        <description>Create unit and integration tests to validate image upload handling, Base64 conversion, external API integration, and response correctness.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>6</id>
        <name>Fix implicit array typing for content property</name>
        <description>Adjust the request payload definition so that the 'content' array is explicitly typed (e.g., using object[] or a shared DTO) to resolve the 'No best type found for implicitly-typed array' compile error.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>7</id>
        <name>Resolve build errors in ImageTaggingController</name>
        <description>Investigate and fix compilation errors in the ImageTaggingController file, including correcting raw string literal usage, ensuring the API endpoint path is accurate, and updating project language version or configuration to support the features used.</description>
        <completed>true</completed>
      </subtask>
      <subtask>
        <id>8</id>
        <name>Refactor request JSON to simple string format</name>
        <description>Update the payload building logic in ImageTaggingController so the 'content' property is a simple string concatenating the text prompt and base64 image data, rather than an implicitly-typed array of objects.</description>
        <completed>true</completed>
      </subtask>
    </subtasks>
  </task>
</tasks>