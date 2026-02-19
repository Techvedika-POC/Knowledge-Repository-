import React, { useEffect, useState } from "react";
import api from "../api";

export default function CodingWorkspace({ problems }) {
  const [activeProblem, setActiveProblem] = useState(null);
  const [code, setCode] = useState("");
  const [aiStatus, setAiStatus] = useState("");
  const [aiResult, setAiResult] = useState(null);
  const [interviewCompleted, setInterviewCompleted] = useState(false);
  const [finalResult, setFinalResult] = useState(null);
  const [showInterviewModal, setShowInterviewModal] = useState(false);
  const [showFinalModal, setShowFinalModal] = useState(false);

  const [showSubmitGuide, setShowSubmitGuide] = useState(false);
  const [showInterviewGuide, setShowInterviewGuide] = useState(false);
  const [showCodeReview, setShowCodeReview] = useState(false);

  const [interviewId, setInterviewId] = useState(null);
  const [chat, setChat] = useState([]);
  const [chatInput, setChatInput] = useState("");

  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (problems.length > 0) {
      setActiveProblem(problems[0]);
    }
  }, [problems]);

  const getUserId = () => {
    const id = localStorage.getItem("userId");
    if (!id) throw new Error("User not logged in");
    return id;
  };
  const submitCode = async () => {
    if (!code.trim() || !activeProblem) return;

    setLoading(true);
    setAiStatus("AI is reviewing your code...");
    setAiResult(null);

    try {
      const res = await api.post("/coding/submit", {
        problemId: activeProblem.problemId,
        userId: getUserId(),
        sourceCode: code,
        language: "C#"
      });

      setAiResult({
        score: res.data.score,
        feedback: res.data.feedback
      });

      setShowCodeReview(true);
      setAiStatus("Review completed");
    } catch (err) {
      console.error(err);
      setAiStatus("Review failed");
    } finally {
      setLoading(false);
    }
  };
  const startInterview = async () => {
    try {
      const res = await api.post("/interview/start", {
        userId: getUserId(),
        problemId: activeProblem.problemId
      });
      setInterviewId(res.data);

      setChat([
        {
          role: "ai",
          user: "AI Interviewer",
          text: "Let's begin. Explain your approach.",
          time: new Date().toLocaleString()
        }
      ]);

      setShowInterviewModal(true);
    } catch (err) {
      console.error(err);
      alert("Failed to start interview. Submit code first.");
    }
  };


  const sendInterviewMessage = async () => {
    if (!chatInput.trim()) return;

    const userMsg = chatInput;
    setChatInput("");

    setChat(prev => [
      ...prev,
      {
        role: "user",
        user: "You",
        text: userMsg,
        time: new Date().toLocaleString()
      }
    ]);

    const res = await api.post("/interview/message", {
      interviewId,
      message: userMsg
    });
    if (res.data.isCompleted) {
      setFinalResult({
        communicationScore: res.data.communicationScore,
        aiFeedback: res.data.aiFeedback
      });

      setShowInterviewModal(false);
      setShowFinalModal(true);
      setInterviewId(null);
      return;
    }
    setChat(prev => [
      ...prev,
      {
        role: "ai",
        user: "AI Interviewer",
        text: res.data.aiQuestion,
        time: new Date().toLocaleString()
      }
    ]);
  };



  return (
    <div className="bg-white rounded-xl shadow-lg p-6 space-y-6">
      <div className="border-b pb-2">
        <h2 className="text-xl font-semibold">Coding Challenge Workspace</h2>
        <p className="text-sm text-gray-500">
          Solve the problem, submit for AI review, then try the AI interview.
        </p>
      </div>
      <div className="flex gap-2 flex-wrap">
        {problems.map(p => (
          <button
            key={p.problemId}
            onClick={() => setActiveProblem(p)}
            className={`px-3 py-1 rounded text-sm ${activeProblem?.problemId === p.problemId
              ? "bg-blue-600 text-white"
              : "bg-gray-200"
              }`}
          >
            {p.title}
          </button>
        ))}
      </div>
      {activeProblem && (
        <div className="bg-gray-100 p-4 rounded text-sm whitespace-pre-wrap">
          {activeProblem.problemStatement}
        </div>
      )}
      <textarea
        className="w-full h-56 border rounded p-3 font-mono"
        value={code}
        onChange={e => setCode(e.target.value)}
        placeholder="Write your solution here..."
      />
      <div className="flex gap-3">
        <button
          onClick={() => setShowSubmitGuide(true)}
          className="bg-green-600 text-white px-4 py-2 rounded"
        >
          Submit Code
        </button>

        <button
          onClick={() => setShowInterviewGuide(true)}
          className="bg-purple-600 text-white px-4 py-2 rounded"
        >
          Start AI Interview
        </button>
      </div>

      {showCodeReview && aiResult && (
        <Modal
          title="AI Code Review Report"
          onClose={() => setShowCodeReview(false)}
          onConfirm={() => setShowCodeReview(false)}
        >
          <div className="space-y-3 text-sm">
            <div className="font-semibold text-lg">
              Score: {aiResult.score}/100
            </div>

            <pre className="whitespace-pre-wrap bg-gray-50 p-3 rounded">
              {aiResult.feedback}
            </pre>
          </div>
        </Modal>
      )}


      {showInterviewModal && (
        <Modal
          title="AI Technical Interview"
          onClose={() => setShowInterviewModal(false)}
          onConfirm={() => setShowInterviewModal(false)}
        >
          <div className="h-[60vh] flex flex-col">
            <div className="flex-1 overflow-y-auto space-y-4">
              {chat.map((m, i) => (
                <div key={i} className={`flex ${m.role === "user" ? "justify-end" : "justify-start"}`}>
                  <div className={`max-w-[70%] p-3 rounded-lg text-sm shadow
              ${m.role === "user"
                      ? "bg-blue-600 text-white"
                      : "bg-gray-100 border"}`}
                  >
                    <div className="font-semibold text-xs">
                      {m.user}
                    </div>
                    <div>{m.text}</div>
                    <div className="text-xs opacity-60 mt-1 text-right">
                      {m.time}
                    </div>
                  </div>
                </div>
              ))}
            </div>
            <div className="mt-3 flex gap-2">
              <input
                className="border flex-1 p-2 rounded"
                value={chatInput}
                onChange={e => setChatInput(e.target.value)}
                placeholder="Type your answer..."
              />
              <button
                onClick={sendInterviewMessage}
                className="bg-blue-600 text-white px-4 rounded"
              >
                Send
              </button>
            </div>
          </div>
        </Modal>
      )}

      {showFinalModal && finalResult && (
        <Modal
          title="Interview Evaluation Report"
          onClose={() => setShowFinalModal(false)}
        >
          <div className="space-y-4 text-sm">
            <div className="text-xl font-bold text-green-700">
              Final Score: {finalResult.communicationScore}/100
            </div>

            <pre className="bg-gray-50 p-3 rounded whitespace-pre-wrap">
              {finalResult.aiFeedback}
            </pre>
          </div>
        </Modal>
      )}
      {showSubmitGuide && (
        <Modal
          title="Before you submit"
          confirmText="Submit Code"
          onClose={() => setShowSubmitGuide(false)}
          onConfirm={() => {
            setShowSubmitGuide(false);
            submitCode();
          }}
        >
          <ul className="text-sm space-y-2">
            <li>• Make sure your code compiles</li>
            <li>• Use meaningful variable names</li>
            <li>• Consider edge cases</li>
            <li>• AI will score correctness and efficiency</li>
          </ul>
        </Modal>
      )}
      {showInterviewGuide && (
        <Modal
          title="AI Interview Guidelines"
          onClose={() => setShowInterviewGuide(false)}
          onConfirm={() => {
            setShowInterviewGuide(false);
            startInterview();
          }}
        >
          <ul className="text-sm space-y-2">
            <li>• Think aloud</li>
            <li>• You can ask clarifying questions</li>
            <li>• No copy-paste</li>
            <li>• Interview lasts ~10 minutes</li>
          </ul>
        </Modal>
      )}
      {loading && (
        <div className="fixed inset-0 bg-black/30 flex items-center justify-center">
          <div className="bg-white px-6 py-4 rounded shadow">
            🤖 AI is thinking...
          </div>
        </div>
      )}
    </div>
  );
}
function Modal({ title, children, onClose, onConfirm, confirmText = "Confirm" }) {
  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl w-full max-w-2xl max-h-[85vh] flex flex-col shadow-lg">

        <div className="p-4 border-b font-bold text-lg">
          {title}
        </div>

        <div className="p-4 overflow-y-auto flex-1">
          {children}
        </div>

        <div className="p-4 border-t flex justify-end gap-3">
          <button
            onClick={onClose}
            className="px-4 py-2 border rounded-md hover:bg-gray-100"
          >
            Cancel
          </button>

          {onConfirm && (
            <button
              onClick={onConfirm}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
            >
              {confirmText}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

