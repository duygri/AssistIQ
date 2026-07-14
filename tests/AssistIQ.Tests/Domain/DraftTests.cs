using AssistIQ.Domain.Drafts;
using FluentAssertions;

namespace AssistIQ.Tests.Domain;

public sealed class DraftTests
{
    [Fact]
    public void CreateAiGenerated_WithNoCitations_ShouldNeedCitationReview()
    {
        var draft = Draft.CreateAiGenerated(
            ticketId: Guid.NewGuid(),
            versionNumber: 1,
            generatedAnswer: "You can reset your password from account settings.",
            citations: []);

        draft.Status.Should().Be(DraftStatus.NeedsCitationReview);
    }

    [Fact]
    public void CreateAiGenerated_ShouldCaptureGeneratedAnswerAndCitations()
    {
        var ticketId = Guid.NewGuid();
        var citation = DraftCitation.Create(
            knowledgeDocumentId: Guid.NewGuid(),
            fileName: "password-faq.pdf",
            providerFileId: "file_123",
            quote: "Password resets are available from account settings.",
            providerResultId: "result_1",
            confidence: 0.87m);

        var draft = Draft.CreateAiGenerated(
            ticketId,
            versionNumber: 2,
            generatedAnswer: "You can reset your password from account settings.",
            citations: [citation]);

        draft.TicketId.Should().Be(ticketId);
        draft.VersionNumber.Should().Be(2);
        draft.GeneratedAnswer.Should().Be("You can reset your password from account settings.");
        draft.EditedAnswer.Should().BeNull();
        draft.Status.Should().Be(DraftStatus.Generated);
        draft.Source.Should().Be(DraftSource.AiGenerated);
        draft.Citations.Should().ContainSingle();
    }

    [Fact]
    public void Edit_ShouldMoveGeneratedDraftToEdited()
    {
        var draft = CreateDraft();

        draft.Edit("Here is the polished support reply.");

        draft.Status.Should().Be(DraftStatus.Edited);
        draft.EditedAnswer.Should().Be("Here is the polished support reply.");
    }

    [Fact]
    public void Send_ShouldAllowEditedDraft()
    {
        var draft = CreateDraft();
        draft.Edit("Here is the final reply.");

        draft.Send();

        draft.Status.Should().Be(DraftStatus.Sent);
    }

    [Fact]
    public void Send_ShouldRejectDraftWithNoCitations()
    {
        var draft = Draft.CreateAiGenerated(
            ticketId: Guid.NewGuid(),
            versionNumber: 1,
            generatedAnswer: "This draft needs source review.",
            citations: []);

        var act = draft.Send;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*citation*");
    }

    [Fact]
    public void Edit_ShouldRejectSentDraft()
    {
        var draft = CreateDraft();
        draft.Edit("Here is the final reply.");
        draft.Send();

        var act = () => draft.Edit("Too late.");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Sent*");
    }

    private static Draft CreateDraft()
    {
        var citation = DraftCitation.Create(
            knowledgeDocumentId: Guid.NewGuid(),
            fileName: "billing-faq.pdf",
            providerFileId: "file_789",
            quote: "Billing dates are based on the workspace creation date.",
            providerResultId: "result_2",
            confidence: 0.91m);

        return Draft.CreateAiGenerated(
            ticketId: Guid.NewGuid(),
            versionNumber: 1,
            generatedAnswer: "Your billing date depends on workspace creation.",
            citations: [citation]);
    }
}
