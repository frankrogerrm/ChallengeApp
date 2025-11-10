import { useState, useEffect } from "react";
import axios from "axios";
import styles from "@/styles/components/ShareDocumentModal.module.css";

type ShareEntry = {
  email: string;
  permission: "Read" | "Write";
};

type Props = {
  token: string;
  documentId: number;
  onClose: () => void;
};

export default function ShareDocumentModal({
  token,
  documentId,
  onClose,
}: Props) {
  const [shareEmail, setShareEmail] = useState("");
  const [sharePermission, setSharePermission] = useState<"Read" | "Write">(
    "Read"
  );
  const [sharedUsers, setSharedUsers] = useState<ShareEntry[]>([]);
  const [availableUsers, setAvailableUsers] = useState<
    { email: string; role: string }[]
  >([]);

  const apiBase = process.env.NEXT_PUBLIC_API_URL;

  useEffect(() => {
    const loadData = async () => {
      try {
        const [sharesRes, usersRes] = await Promise.all([
          axios.get<ShareEntry[]>(`${apiBase}/documents/${documentId}/shares`, {
            headers: { Authorization: `Bearer ${token}` },
          }),
          axios.get<{ email: string; role: string }[]>(`${apiBase}/auth/users`),
        ]);
        setSharedUsers(sharesRes.data || []);
        setAvailableUsers(usersRes.data || []);
      } catch {}
    };

    loadData();
  }, [apiBase, documentId, token]);

  const loadShares = async () => {
    try {
      const res = await axios.get<ShareEntry[]>(
        `${apiBase}/documents/${documentId}/shares`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setSharedUsers(res.data || []);
    } catch {}
  };

  const handleShare = async () => {
    if (!shareEmail) return;
    try {
      await axios.post(
        `${apiBase}/documents/${documentId}/share`,
        { emails: [shareEmail], permission: sharePermission },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setShareEmail("");
      await loadShares();
    } catch {
      alert("Failed to share document.");
    }
  };

  const handleRevoke = async (email: string) => {
    try {
      await axios.delete(`${apiBase}/documents/${documentId}/share`, {
        headers: { Authorization: `Bearer ${token}` },
        params: { email },
      });
      await loadShares();
    } catch {
      alert("Failed to revoke access.");
    }
  };

  return (
    <div className={`modal show d-block ${styles.modalBackdrop}`}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Share Document</h5>
            <button
              type="button"
              className="btn-close"
              onClick={onClose}
              aria-label="Close"
            />
          </div>
          <div className="modal-body">
            <div className="mb-3">
              <label htmlFor="userSelect" className="form-label">
                Select user:
              </label>
              <select
                id="userSelect"
                className="form-select"
                value={shareEmail}
                onChange={(e) => setShareEmail(e.target.value)}
              >
                <option value="">-- Select --</option>
                {availableUsers.map((user) => (
                  <option key={user.email} value={user.email}>
                    {user.email} ({user.role})
                  </option>
                ))}
              </select>
            </div>

            <div className="mb-3">
              <label htmlFor="permissionSelect" className="form-label">
                Permission:
              </label>
              <select
                id="permissionSelect"
                className="form-select"
                value={sharePermission}
                onChange={(e) =>
                  setSharePermission(e.target.value as "Read" | "Write")
                }
              >
                <option value="Read">Read</option>
                <option value="Write">Write</option>
              </select>
            </div>

            <div className="mb-3">
              <h6 className="mb-3">Shared With:</h6>
              <div className={`list-group ${styles.userList}`}>
                {sharedUsers.map((entry, i) => (
                  <div
                    key={i}
                    className="list-group-item d-flex justify-content-between align-items-center"
                  >
                    <div>
                      <span className="fw-bold">{entry.email}</span>
                      <span
                        className={`ms-2 badge bg-info ${styles.permissionBadge}`}
                      >
                        {entry.permission}
                      </span>
                    </div>
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => handleRevoke(entry.email)}
                    >
                      Revoke
                    </button>
                  </div>
                ))}
              </div>
            </div>
          </div>
          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
            >
              Cancel
            </button>
            <button
              type="button"
              className="btn btn-primary"
              onClick={handleShare}
              disabled={!shareEmail}
            >
              Share
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
