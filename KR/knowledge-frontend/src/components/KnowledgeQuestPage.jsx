import React, { useEffect, useState } from "react";
import axios from "axios";
import api from "../api";
import { motion } from "framer-motion";
import { Lock, PlayCircle, CheckCircle2, XCircle } from "lucide-react";
import toast from "react-hot-toast";


export default function KnowledgeQuestPage() {
  const [topics, setTopics] = useState([]);
  const [modules, setModules] = useState([]);
  const [selectedTopic, setSelectedTopic] = useState(null);
  const [activeModule, setActiveModule] = useState(null);
  const [questions, setQuestions] = useState([]);
  const [answers, setAnswers] = useState({});
  const [testStarted, setTestStarted] = useState(false);
  const [testCompleted, setTestCompleted] = useState(false);
  const [score, setScore] = useState(0);
  const [loading, setLoading] = useState(false);
  const [videoStarted, setVideoStarted] = useState(false);

  const userId = localStorage.getItem("userId");

  // Load Topics
  useEffect(() => {
    const fetchTopics = async () => {
      try {
        const res = await api.get("/VLearnTopic/all");
        setTopics(res.data);
      } catch {
        toast.error("Failed to load topics");
      }
    };
    fetchTopics();
  }, []);

  const handleTopicSelect = async (topicId) => {
    setSelectedTopic(topicId);
    setModules([]);
    setTestStarted(false);
    setTestCompleted(false);
    setActiveModule(null);
    setLoading(true);
    try {
      const res = await api.get(`/VLearnModule/${topicId}/${userId}`);
      setModules(res.data);
    } catch {
      toast.error("Failed to load modules");
    } finally {
      setLoading(false);
    }
  };

  const handlePlayModule = (mod) => {
    if (mod.isLocked) {
      toast.error("This module is locked. Complete the previous one first!");
      return;
    }
    setActiveModule(mod);
    setVideoStarted(true);
    setTestStarted(false);
    setTestCompleted(false);
  };

  const handleStartTest = async (mod) => {
    if (mod.isLocked) {
      toast.error("This module is locked. Complete the previous one first!");
      return;
    }

    setActiveModule(mod);
    setAnswers({});
    setTestStarted(true);
    setTestCompleted(false);
    setVideoStarted(false);

    try {
      const categoryMap = {
        javascript: "code",
        python: "code",
        sql: "sql",
        linux: "linux",
        devops: "devops",
        docker: "docker",
      };

      const category = categoryMap[mod.moduleName.toLowerCase()] || "code";

      const res = await axios.get("https://quizapi.io/api/v1/questions", {
        params: {
          apiKey: "SPPJhXqvcRBIYXxgZbjjOjGkyLKdBcjI5H2Gqr96",
          category,
          difficulty: "Easy",
          limit: 5,
        },
      });

      if (res.data && res.data.length > 0) {
        const formatted = res.data.map((q) => ({
          question: q.question,
          options: Object.values(q.answers).filter((opt) => opt !== null),
          correctAnswer:
            q.correct_answer ||
            Object.keys(q.correct_answers).find((k) => q.correct_answers[k] === "true"),
        }));
        setQuestions(formatted);
      } else {
        toast.error("No questions found for this topic.");
      }
    } catch (err) {
      console.error(err);
      toast.error("Failed to load questions from QuizAPI.io.");
    }
  };

  const handleAnswerChange = (qIndex, option) => {
    setAnswers({ ...answers, [qIndex]: option });
  };

  const handleSubmitTest = async () => {
    let correct = 0;
    questions.forEach((q, index) => {
      if (answers[index]?.toLowerCase().includes(q.correctAnswer?.slice(-1)?.toLowerCase())) {
        correct++;
      }
    });

    const percentage = (correct / questions.length) * 100;
    setScore(percentage);
    setTestCompleted(true);
    setTestStarted(false);

    const testStatus = percentage >= 60 ? "Passed" : "Failed";
    toast.success(`You scored ${percentage.toFixed(1)}% - ${testStatus}`);

    try {
      await api.post("/VLearnModule/update-test-status", {
        userId,
        topicId: selectedTopic,
        moduleId: activeModule.moduleId,
        testStatus,
      });

      const res = await api.get(`/VLearnModule/${selectedTopic}/${userId}`);
      setModules(res.data);
    } catch (err) {
      console.error(err);
      toast.error("Failed to update test progress.");
    }
  };

  const extractYouTubeId = (url) => {
    const match = url?.match(
      /(?:youtu\.be\/|youtube\.com\/(?:watch\?v=|embed\/))([^&?/]+)/i
    );
    return match ? match[1] : null;
  };

  // Image Mapping Helper (Case + Space insensitive)
  const normalize = (name) => name?.toLowerCase().replace(/\s+/g, "");

  const topicImages = {
    datascience: "/assets/datascience.png",
    frontenddevelopment: "/assets/frontend.png",
    backenddevelopment: "/assets/backend.png",
    artificialintelligence: "/assets/ai.png",
    devops: "/assets/devops.png",
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-white to-blue-50 pb-20">
      {/*  Heading */}
      <motion.div
        className="text-center mb-10"
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <h1 className="text-4xl font-extrabold text-blue-700 mb-4">VLearn Knowledge Progression</h1>
       
        <p className="text-gray-600 max-w-2xl mx-auto text-lg">
          Select a topic, watch, learn, and complete interactive tests to unlock the next module.
        </p>
      </motion.div>
<div className="w-full flex justify-center gap-6 px-6 py-10">
  {[
    {
      img: "/assets/lms1.png",
      title: "Think Innovatively",
      desc: "Unlock your learning potential through interactive modules and quizzes.",
    },
    {
      img: "/assets/Assessment.png",
      title: "Step Into the Future",
      desc: "Experience a smarter way of learning through guided knowledge paths.",
    },
    {
      img: "/assets/lms.png",
      title: "Empower Your Skills",
      desc: "Learn, test, and grow through structured VLearn Knowledge Quest.",
    },
  ].map((slide, i) => (
    <div
      key={i}
      className="relative w-[30%] h-[220px] rounded-2xl overflow-hidden shadow-md bg-cover bg-center"
      style={{ backgroundImage: `url(${slide.img})` }}
    >
      <div className="absolute inset-0 bg-black/40"></div>
      <div className="absolute inset-0 flex flex-col justify-center items-center text-white px-4 text-center">
        <h1 className="text-lg font-semibold mb-1">{slide.title}</h1>
        <p className="text-xs sm:text-sm">{slide.desc}</p>
      </div>
    </div>
  ))}
</div>

 

   {/*Topics Section  */}
<div className="max-w-6xl mx-auto mb-12 px-4">
  <h2 className="text-2xl font-semibold text-center text-blue-700 mb-6">
    Select a Topic
  </h2>

  <div className="grid md:grid-cols-3 sm:grid-cols-2 gap-6">
    {topics.map((t) => (
      <motion.div
        key={t.topicId}
        onClick={() => handleTopicSelect(t.topicId)}
        whileHover={{ scale: 1.03 }}
        className={`cursor-pointer bg-white rounded-2xl shadow-md border transition-all 
          ${
            selectedTopic === t.topicId
              ? "border-blue-400 ring-2 ring-blue-300"
              : "hover:shadow-lg"
          }`}
      >
        <div className="p-6 flex flex-col justify-between h-full">
          <div>
            <h3 className="text-xl font-semibold text-blue-800 mb-2">
              {t.topicName}
            </h3>
            <p className="text-gray-600 text-sm line-clamp-3">
              {t.description || "No description available for this topic."}
            </p>
          </div>

          {/* Optional status or progress indicator */}
          <div className="mt-4">
            <span className="text-xs text-gray-500">
              {t.moduleCount
                ? `${t.moduleCount} Modules`
                : "Explore modules inside"}
            </span>
          </div>
        </div>
      </motion.div>
    ))}
  </div>
</div>


      {/* Modules Section */}
      {selectedTopic && (
        <motion.div
          className="max-w-4xl mx-auto bg-white rounded-2xl shadow-lg p-8 border border-blue-100"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
        >
          <h2 className="text-2xl font-semibold text-blue-700 mb-6 text-center">Modules</h2>
          {loading ? (
            <p className="text-center text-gray-500">Loading modules...</p>
          ) : (
            <ul className="space-y-4">
              {modules.map((mod, index) => (
                <li
                  key={mod.moduleId}
                  className="flex justify-between items-center bg-blue-50 hover:bg-blue-100 transition rounded-xl p-4"
                >
                  <div>
                    <p className="font-semibold text-blue-800">
                      {index + 1}. {mod.moduleName}
                    </p>
                    <p className="text-sm text-gray-600">{mod.description}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    {mod.isLocked ? (
                      <button
                        disabled
                        className="flex items-center gap-1 bg-gray-200 text-gray-500 px-4 py-2 rounded-lg cursor-not-allowed"
                      >
                        <Lock size={16} /> Locked
                      </button>
                    ) : (
                      <>
                        <button
                          onClick={() => handlePlayModule(mod)}
                          className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition"
                        >
                          <PlayCircle size={18} /> Play Video
                        </button>
                        <button
                          onClick={() => handleStartTest(mod)}
                          className="flex items-center gap-2 bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 transition"
                        >
                           Take Test
                        </button>
                      </>
                    )}
                  </div>
                </li>
              ))}
            </ul>
          )}
        </motion.div>
      )}

      {/*  Video Section */}
      {videoStarted && activeModule && (
        <motion.div
          className="max-w-3xl mx-auto mt-12 bg-white rounded-2xl shadow-xl p-8 border border-blue-100"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
        >
          <h3 className="text-2xl font-semibold text-blue-700 mb-4 text-center">
            {activeModule.moduleName} - Video Lesson
          </h3>

          {extractYouTubeId(activeModule.contentLink) ? (
            <iframe
              width="100%"
              height="400"
              src={`https://www.youtube.com/embed/${extractYouTubeId(activeModule.contentLink)}`}
              title="Video Player"
              frameBorder="0"
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
              className="rounded-xl w-full shadow-md"
            />
          ) : (
            <video
              src={activeModule.contentLink || "https://www.w3schools.com/html/mov_bbb.mp4"}
              controls
              className="rounded-xl w-full shadow-md"
            />
          )}
        </motion.div>
      )}

      {/*  Test Section */}
      {testStarted && (
        <motion.div
          className="max-w-3xl mx-auto mt-12 bg-white rounded-2xl shadow-xl p-8 border border-blue-100"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
        >
          <h3 className="text-2xl font-semibold text-blue-700 mb-6 text-center">
            {activeModule?.moduleName} - Test
          </h3>
          {questions.map((q, i) => (
            <div key={i} className="mb-6">
              <p className="font-medium text-gray-800 mb-2">
                {i + 1}. {q.question}
              </p>
              <div className="space-y-2">
                {q.options.map((opt, j) => (
                  <label key={j} className="block cursor-pointer">
                    <input
                      type="radio"
                      name={`q${i}`}
                      value={opt}
                      checked={answers[i] === opt}
                      onChange={() => handleAnswerChange(i, opt)}
                      className="mr-2"
                    />
                    {opt}
                  </label>
                ))}
              </div>
            </div>
          ))}
          <div className="text-center">
            <button
              onClick={handleSubmitTest}
              className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 transition"
            >
              Submit Test
            </button>
          </div>
        </motion.div>
      )}

      {/* Test Result */}
      {testCompleted && (
        <motion.div className="text-center mt-12" initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          <h3 className="text-3xl font-bold text-blue-700 mb-3">Test Completed!</h3>
          <p className="text-lg text-gray-700 mb-3">Your Score: {score.toFixed(2)}%</p>
          {score >= 60 ? (
            <div className="flex justify-center items-center gap-2 text-green-600 font-semibold">
              <CheckCircle2 size={28} /> You Passed 🎉
            </div>
          ) : (
            <div className="flex justify-center items-center gap-2 text-red-600 font-semibold">
              <XCircle size={28} /> You Failed 
            </div>
          )}
        </motion.div>
      )}
    </div>
  );
}