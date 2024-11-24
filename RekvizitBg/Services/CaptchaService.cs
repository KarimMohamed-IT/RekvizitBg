namespace RekvizitBg.Services
{
    using System;
    using Google.Api.Gax.ResourceNames;
    using Google.Cloud.RecaptchaEnterprise.V1;
    using Microsoft.Extensions.Logging;

    public class CreateAssessmentSample
    {
        // Create an assessment to analyze the risk of a UI action.
        // projectID: Your Google Cloud Project ID.
        // recaptchaKey: The reCAPTCHA key associated with the site/app
        // token: The generated token obtained from the client.
        // recaptchaAction: Action name corresponding to the token.
        public void createAssessment(string projectID = "custom-hold-303015", string recaptchaKey = "6LcC9IUqAAAAAL6JE7blweCaalkMn2hCtyWUlWIh", string token = "action-token", string recaptchaAction = "action-name")
        {
            // Create the reCAPTCHA client.
            // TODO: Cache the client generation code (recommended) or call client.close() before exiting the method.
            RecaptchaEnterpriseServiceClient client = RecaptchaEnterpriseServiceClient.Create();

            ProjectName projectName = new ProjectName(projectID);

            // Build the assessment request.
            CreateAssessmentRequest createAssessmentRequest = new CreateAssessmentRequest()
            {
                Assessment = new Assessment()
                {
                    // Set the properties of the event to be tracked.
                    Event = new Event()
                    {
                        SiteKey = recaptchaKey,
                        Token = token,
                        ExpectedAction = recaptchaAction
                    },
                },
                ParentAsProjectName = projectName
            };

            Assessment response = client.CreateAssessment(createAssessmentRequest);

            // Check if the token is valid.
            if (response.TokenProperties.Valid == false)
            {
                System.Console.WriteLine("The CreateAssessment call failed because the token was: " +
                    response.TokenProperties.InvalidReason.ToString());
                return;
            }

            // Check if the expected action was executed.
            if (response.TokenProperties.Action != recaptchaAction)
            {
                System.Console.WriteLine("The action attribute in reCAPTCHA tag is: " +
                    response.TokenProperties.Action.ToString());
                System.Console.WriteLine("The action attribute in the reCAPTCHA tag does not " +
                    "match the action you are expecting to score");
                return;
            }

            // Get the risk score and the reason(s).
            // For more information on interpreting the assessment, see:
            // https://cloud.google.com/recaptcha-enterprise/docs/interpret-assessment
            System.Console.WriteLine("The reCAPTCHA score is: " + ((decimal)response.RiskAnalysis.Score));

            foreach (RiskAnalysis.Types.ClassificationReason reason in response.RiskAnalysis.Reasons)
            {
                System.Console.WriteLine(reason.ToString());
            }
        }
    }
}