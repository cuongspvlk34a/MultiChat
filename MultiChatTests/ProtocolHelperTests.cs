using System;
using System.Collections.Generic;
using Xunit;

// ──────────────────────────────────────────────────────────────────────────────
// MultiChatTests/ProtocolHelperTests.cs
// Tests cover BOTH MultiChatClient.ProtocolHelper and MultiChatServer.ProtocolHelper
// Run: dotnet test
// ──────────────────────────────────────────────────────────────────────────────

// ─── actual test class ─────────────────────────────────────────────────────────

namespace MultiChatTests
{
    public class ProtocolHelperTests
    {
        // ── Aliases so test bodies stay readable ─────────────────────────────

        // ════════════════════════════════════════════════════════════════════
        // 1. CLIENT — BuildMessage
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Client_BuildMessage_NormalInput_ReturnsCorrectFormat()
        {
            var result = MultiChatClient.ProtocolHelper.BuildMessage("Alice", "Hello");
            Assert.Equal("MSG|Alice|Hello", result);
        }

        [Fact]
        public void Client_BuildMessage_PipeInUserName_IsSanitized()
        {
            // Pipe in userName must be replaced so the protocol frame stays intact.
            var result = MultiChatClient.ProtocolHelper.BuildMessage("Ali|ce", "Hi");
            Assert.Equal("MSG|Ali/ce|Hi", result);
            // Exactly 2 pipes → only 3 segments
            Assert.Equal(3, result.Split('|').Length);
        }

        [Fact]
        public void Client_BuildMessage_CommaInMessage_IsPreserved()
        {
            // SanitizeMessage must NOT strip commas — users can write "Hello, world".
            var result = MultiChatClient.ProtocolHelper.BuildMessage("Alice", "Hello, world");
            Assert.Contains("Hello, world", result);
        }

        [Fact]
        public void Client_BuildMessage_NewlineInMessage_IsStripped()
        {
            var result = MultiChatClient.ProtocolHelper.BuildMessage("Alice", "line1\nline2");
            Assert.DoesNotContain("\n", result);
            Assert.Contains("line1 line2", result);
        }

        [Fact]
        public void Client_BuildMessage_CarriageReturnInMessage_IsStripped()
        {
            var result = MultiChatClient.ProtocolHelper.BuildMessage("Bob", "a\r\nb");
            Assert.DoesNotContain("\r", result);
            Assert.DoesNotContain("\n", result);
        }

        // ════════════════════════════════════════════════════════════════════
        // 2. CLIENT — BuildReply
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Client_BuildReply_NormalInput_HasCorrectSegments()
        {
            var result = MultiChatClient.ProtocolHelper.BuildReply("Alice", "msg-001", "Thanks!");
            var parts = result.Split('|');
            Assert.Equal("REPLY", parts[0]);
            Assert.Equal("Alice",   parts[1]);
            Assert.Equal("msg-001", parts[2]);
            Assert.Equal("Thanks!", parts[3]);
        }

        [Fact]
        public void Client_BuildReply_PipeInMessage_IsSanitized()
        {
            var result = MultiChatClient.ProtocolHelper.BuildReply("Bob", "id1", "trick|value");
            // Message segment must not introduce extra pipes
            var parts = result.Split('|');
            Assert.Equal(4, parts.Length);
            Assert.Equal("trick/value", parts[3]);
        }

        [Fact]
        public void Client_BuildReply_CommaInMessage_IsPreserved()
        {
            var result = MultiChatClient.ProtocolHelper.BuildReply("Alice", "id2", "Yes, please");
            Assert.Contains("Yes, please", result);
        }

        // ════════════════════════════════════════════════════════════════════
        // 3. CLIENT — BuildUserList
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Client_BuildUserList_CommaInUserName_IsStrippedFromField()
        {
            // If a username contains a comma it would break the comma-separated list.
            var result = MultiChatClient.ProtocolHelper.BuildUserList(new[] { "Ali,ce", "Bob" });
            // Comma in the username should be replaced with space
            Assert.Equal("USERS|Ali ce,Bob", result);
        }

        [Fact]
        public void Client_BuildUserList_EmptyList_ReturnsUsersBarEmpty()
        {
            var result = MultiChatClient.ProtocolHelper.BuildUserList(Array.Empty<string>());
            Assert.Equal("USERS|", result);
        }

        [Fact]
        public void Client_BuildUserList_MultipleUsers_JoinedByComma()
        {
            var result = MultiChatClient.ProtocolHelper.BuildUserList(new[] { "Alice", "Bob", "Carol" });
            Assert.Equal("USERS|Alice,Bob,Carol", result);
        }

        // ════════════════════════════════════════════════════════════════════
        // 4. CLIENT — Parse
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Client_Parse_WellFormedLine_ReturnsCorrectSegments()
        {
            var parts = MultiChatClient.ProtocolHelper.Parse("MSG|Alice|msg-1|Hello world");
            Assert.Equal(new[] { "MSG", "Alice", "msg-1", "Hello world" }, parts);
        }

        [Fact]
        public void Client_Parse_EmptyString_ReturnsSingleEmptySegment()
        {
            // Split('|') on "" returns [""] — one empty element, not zero
            var parts = MultiChatClient.ProtocolHelper.Parse("");
            Assert.Single(parts);
            Assert.Equal("", parts[0]);
        }

        [Fact]
        public void Client_Parse_NoDelimiter_ReturnsSingleElement()
        {
            var parts = MultiChatClient.ProtocolHelper.Parse("PING");
            Assert.Single(parts);
            Assert.Equal("PING", parts[0]);
        }

        [Fact]
        public void Client_Parse_TrailingPipe_ProducesEmptyLastSegment()
        {
            // "JOIN|" → ["JOIN", ""]  — caller must guard against empty fields
            var parts = MultiChatClient.ProtocolHelper.Parse("JOIN|");
            Assert.Equal(2, parts.Length);
            Assert.Equal("", parts[1]);
        }

        // ════════════════════════════════════════════════════════════════════
        // 5. SERVER — BuildMessage (server assigns messageId)
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Server_BuildMessage_NormalInput_HasFourSegments()
        {
            var result = MultiChatServer.ProtocolHelper.BuildMessage("Alice", "msg-42", "Hey there");
            var parts = result.Split('|');
            Assert.Equal(4, parts.Length);
            Assert.Equal("MSG",     parts[0]);
            Assert.Equal("Alice",   parts[1]);
            Assert.Equal("msg-42",  parts[2]);
            Assert.Equal("Hey there", parts[3]);
        }

        [Fact]
        public void Server_BuildMessage_CommaInBody_IsPreserved()
        {
            var result = MultiChatServer.ProtocolHelper.BuildMessage("Alice", "id1", "one, two, three");
            Assert.Contains("one, two, three", result);
        }

        // ════════════════════════════════════════════════════════════════════
        // 6. SERVER — BuildReply
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Server_BuildReply_NormalInput_HasFiveSegments()
        {
            var result = MultiChatServer.ProtocolHelper.BuildReply("Bob", "new-10", "orig-5", "Got it!");
            var parts = result.Split('|');
            Assert.Equal(5, parts.Length);
            Assert.Equal("REPLY",  parts[0]);
            Assert.Equal("Bob",    parts[1]);
            Assert.Equal("new-10", parts[2]);
            Assert.Equal("orig-5", parts[3]);
            Assert.Equal("Got it!", parts[4]);
        }

        // ════════════════════════════════════════════════════════════════════
        // 7. SERVER — BuildUserList (same comma-sanitisation contract)
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Server_BuildUserList_PipeInUserName_IsSanitized()
        {
            var result = MultiChatServer.ProtocolHelper.BuildUserList(new[] { "Ali|ce" });
            // pipe → "/" and the result must still be parseable
            Assert.Equal("USERS|Ali/ce", result);
        }

        // ════════════════════════════════════════════════════════════════════
        // 8. SanitizeField vs SanitizeMessage — observable via public builders
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void SanitizeField_CommaInUserName_IsReplaced()
        {
            // BuildJoin routes through SanitizeField → comma must become space
            var result = MultiChatClient.ProtocolHelper.BuildJoin("Ali,ce");
            Assert.Equal("JOIN|Ali ce", result);
        }

        [Fact]
        public void SanitizeMessage_CommaInMessage_IsKept()
        {
            // BuildMessage body routes through SanitizeMessage → comma preserved
            var result = MultiChatClient.ProtocolHelper.BuildMessage("Alice", "a,b,c");
            Assert.Equal("MSG|Alice|a,b,c", result);
        }

        [Fact]
        public void SanitizeField_And_SanitizeMessage_BothStripPipe()
        {
            // Both sanitisers must strip "|" regardless of position
            var fieldResult   = MultiChatClient.ProtocolHelper.BuildJoin("A|B");
            var messageResult = MultiChatClient.ProtocolHelper.BuildMessage("X", "Hello|World");

            Assert.DoesNotContain("A|B",          fieldResult);
            Assert.DoesNotContain("Hello|World",  messageResult);
            Assert.Contains("A/B",         fieldResult);
            Assert.Contains("Hello/World", messageResult);
        }

        // ════════════════════════════════════════════════════════════════════
        // 9. Additional edge-cases
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Client_BuildPrivateMessage_CommaInMessage_IsPreserved()
        {
            var result = MultiChatClient.ProtocolHelper.BuildPrivateMessage("Alice", "Bob", "Hi, Bob!");
            var parts = result.Split('|');
            Assert.Equal("PM", parts[0]);
            Assert.Equal("Hi, Bob!", parts[3]);
        }

        [Fact]
        public void Server_BuildSystem_PipeInMessage_IsSanitized()
        {
            var result = MultiChatServer.ProtocolHelper.BuildSystem("Server|restarting");
            Assert.Equal("SYS|Server/restarting", result);
        }

        [Fact]
        public void Client_BuildRecall_PipeInMessageId_IsSanitized()
        {
            var result = MultiChatClient.ProtocolHelper.BuildRecall("Alice", "id|bad");
            var parts = result.Split('|');
            // Should be exactly 3 segments: RECALL, userName, messageId
            Assert.Equal(3, parts.Length);
            Assert.Equal("id/bad", parts[2]);
        }

        [Fact]
        public void Server_Parse_MalformedExtraDelimiters_StillSplits()
        {
            // A rogue extra pipe should not throw — caller inspects length
            var parts = MultiChatServer.ProtocolHelper.Parse("MSG|Alice||broken|extra");
            Assert.Equal(5, parts.Length);
            Assert.Equal("", parts[2]); // empty segment between double-pipe
        }

        [Fact]
        public void Client_BuildTyping_NewlineInName_IsStripped()
        {
            var result = MultiChatClient.ProtocolHelper.BuildTyping("Ali\nce");
            Assert.DoesNotContain("\n", result);
            Assert.Equal("TYPING|Ali ce", result);
        }
    }
}
