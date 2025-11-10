import { useEffect, useState, useCallback } from "react";
import axios from "axios";
import { useRouter } from "next/router";
import { jwtDecode } from "jwt-decode";
import { extractClaims } from "@/utils/extractclaims";
import EditDocumentModal from "@/components/editdocumentmodal";
import UploadModal from "@/components/uploadmodal";
import ShareDocumentModal from "@/components/sharedocumentmodal";
import styles from "@/styles/components/Dashboard.module.css";
import ViewDocumentDetailsModal from "@/components/viewdocumentdetailsmodal";

type TagDto = { name: string };

type DocumentDto = {
  id: number;
  title: string;
  accessType: string;
  createdDate: string;
  uploadedBy: string;
  tags: TagDto[];
};

type PagedResponse = {
  documents: DocumentDto[];
  totalPages: number;
};

export default function DashboardPage() {
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [showModal, setShowModal] = useState(false);

  const [userRole, setUserRole] = useState("Admin");
  const [userEmail, setUserEmail] = useState("admin@company.com");
  const [sessionReady, setSessionReady] = useState(true);
  const [editingDoc, setEditingDoc] = useState<DocumentDto | null>(null);
  const [shareModalDoc, setShareModalDoc] = useState<DocumentDto | null>(null);
  const [viewingDocId, setViewingDocId] = useState<number | null>(null);

  const router = useRouter();
  const token =
    typeof window !== "undefined" ? localStorage.getItem("token") : null;

  useEffect(() => {
    if (typeof window !== "undefined") {
      const storedToken = localStorage.getItem("token");

      if (!storedToken) {
        router.push("/");
        return;
      }

      try {
        const { role, email } = extractClaims(storedToken);
        const decoded = jwtDecode<{ exp?: number }>(storedToken);

        if (
          !role ||
          !email ||
          (decoded.exp && Date.now() >= decoded.exp * 1000)
        ) {
          localStorage.removeItem("token");
          router.push("/");
          return;
        }

        setUserRole(role);
        setUserEmail(email);
        setSessionReady(true);
      } catch {
        localStorage.removeItem("token");
        router.push("/");
      }
    }
  }, [router]);

  const handleLogout = () => {
    localStorage.removeItem("token");
    router.push("/");
  };

  const fetchDocuments = useCallback(async () => {
    if (!token) {
      router.push("/");
      return;
    }

    setLoading(true);
    try {
      const url = search.trim()
        ? `${process.env.NEXT_PUBLIC_API_URL}/documents/search`
        : `${process.env.NEXT_PUBLIC_API_URL}/documents`;

      const response = await axios.get<PagedResponse>(url, {
        headers: { Authorization: `Bearer ${token}` },
        params: { query: search, page },
      });

      const { documents, totalPages } = response.data || {};
      setDocuments(Array.isArray(documents) ? documents : []);
      setTotalPages(totalPages || 1);
      setError("");
    } catch {
      setError("Failed to load documents.");
    } finally {
      setLoading(false);
    }
  }, [token, router, search, page]);

  const handleDownload = async (id: number) => {
    if (!token) return;

    try {
      const url = `${process.env.NEXT_PUBLIC_API_URL}/documents/download/${id}`;
      const response = await axios.get(url, {
        headers: { Authorization: `Bearer ${token}` },
        responseType: "blob",
      });

      const disposition = response.headers["content-disposition"];
      let filename = "document";

      if (disposition) {
        const filenameStarMatch = disposition.match(
          /filename\*=UTF-8''([^;]+)/
        );
        const filenameMatch = disposition.match(/filename="?([^"]+)"?/);
        filename = decodeURIComponent(
          filenameStarMatch?.[1] || filenameMatch?.[1] || filename
        );
      }

      const blob = new Blob([response.data as BlobPart]);
      const link = document.createElement("a");
      link.href = URL.createObjectURL(blob);
      link.download = filename;
      link.click();
      URL.revokeObjectURL(link.href);
    } catch {
      alert("Failed to download document.");
    }
  };

  const handleEdit = (doc: DocumentDto) => {
    setEditingDoc(doc);
  };

  const handleDelete = async (id: number) => {
    if (!token) return;
    const confirmed = window.confirm(
      "Are you sure you want to delete this document?"
    );
    if (!confirmed) return;

    try {
      await axios.delete(`${process.env.NEXT_PUBLIC_API_URL}/documents/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchDocuments();
    } catch {
      alert("Failed to delete document.");
    }
  };

  useEffect(() => {
    fetchDocuments();
  }, [fetchDocuments]);

  if (!sessionReady) return <p>Loading session...</p>;

  return (
    <div className={styles.dashboard}>
      <div className={styles.header}>
        <div>
          <h2 className="mb-4">Documents</h2>
        </div>
        <div className={styles.userInfo}>
          <div className="mb-2">
            <strong>Email:</strong> {userEmail}
          </div>
          <div className="mb-2">
            <strong>Role:</strong> {userRole}
          </div>
          <div>
            <button className="btn btn-danger btn-sm" onClick={handleLogout}>
              Logout
            </button>
          </div>
        </div>
      </div>

      <div className={styles.searchSection}>
        <div className={styles.searchBar}>
          <div className="input-group mb-0">
            <input
              type="text"
              className="form-control"
              placeholder="Search by title or access type"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            {search && (
              <button
                className="btn btn-outline-secondary"
                type="button"
                onClick={() => setSearch("")}
              >
                Clear
              </button>
            )}
          </div>
        </div>
        <div>
          {["Admin", "Contributor"].includes(userRole) && (
            <button
              className="btn btn-primary"
              onClick={() => setShowModal(true)}
            >
              <i className="bi bi-upload me-2"></i>Upload new document
            </button>
          )}
        </div>
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : error ? (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      ) : (
        <div className={`table-responsive ${styles.tableContainer}`}>
          <table className="table table-hover">
            <thead className="table-light">
              <tr>
                <th>Title</th>
                <th>Access</th>
                <th>Date</th>
                <th>Uploaded By</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {documents.length > 0 ? (
                documents.map((doc) => (
                  <tr key={doc.id}>
                    <td>{doc.title}</td>
                    <td>
                      <span
                        className={`badge bg-${
                          doc.accessType === "Public"
                            ? "success"
                            : doc.accessType === "Private"
                            ? "danger"
                            : "warning"
                        }`}
                      >
                        {doc.accessType}
                      </span>
                    </td>
                    <td>{new Date(doc.createdDate).toLocaleDateString()}</td>
                    <td>{doc.uploadedBy}</td>
                    <td>
                      <div className={styles.actionButtons}>
                        <button
                          className="btn btn-sm btn-outline-dark"
                          onClick={() => setViewingDocId(doc.id)}
                        >
                          <i className="bi bi-info-circle"></i>
                        </button>

                        {(userRole === "Admin" ||
                          userRole === "Contributor" ||
                          userRole === "Manager" ||
                          (userRole === "Viewer" &&
                            doc.accessType === "Public")) && (
                          <button
                            className="btn btn-sm btn-outline-primary"
                            onClick={() => handleDownload(doc.id)}
                          >
                            <i className="bi bi-download"></i>
                          </button>
                        )}
                        {(userRole === "Admin" ||
                          doc.uploadedBy === userEmail) && (
                          <button
                            className="btn btn-sm btn-outline-info"
                            onClick={() => setShareModalDoc(doc)}
                          >
                            <i className="bi bi-share"></i>
                          </button>
                        )}
                        {(userRole === "Admin" ||
                          (userRole === "Contributor" &&
                            doc.uploadedBy === userEmail)) && (
                          <>
                            <button
                              className="btn btn-sm btn-outline-secondary"
                              onClick={() => handleEdit(doc)}
                            >
                              <i className="bi bi-pencil"></i>
                            </button>
                            <button
                              className="btn btn-sm btn-outline-danger"
                              onClick={() => handleDelete(doc.id)}
                            >
                              <i className="bi bi-trash"></i>
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className="text-center py-4">
                    No documents available.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      <nav className={styles.pagination}>
        <ul className="pagination">
          <li className={`page-item ${page === 1 ? "disabled" : ""}`}>
            <button
              className="page-link"
              onClick={() => setPage((p) => Math.max(p - 1, 1))}
              disabled={page === 1}
            >
              Previous
            </button>
          </li>
          <li className="page-item">
            <span className="page-link">
              Page {page} of {totalPages}
            </span>
          </li>
          <li className={`page-item ${page === totalPages ? "disabled" : ""}`}>
            <button
              className="page-link"
              onClick={() => setPage((p) => Math.min(p + 1, totalPages))}
              disabled={page === totalPages}
            >
              Next
            </button>
          </li>
        </ul>
      </nav>

      {showModal && token && (
        <UploadModal
          token={token}
          onClose={() => setShowModal(false)}
          onSuccess={fetchDocuments}
        />
      )}

      {editingDoc && token && (
        <EditDocumentModal
          token={token}
          document={editingDoc}
          onClose={() => setEditingDoc(null)}
          onSuccess={() => {
            setEditingDoc(null);
            fetchDocuments();
          }}
        />
      )}

      {shareModalDoc && token && (
        <ShareDocumentModal
          token={token}
          documentId={shareModalDoc.id}
          onClose={() => setShareModalDoc(null)}
        />
      )}

      {viewingDocId && token && (
        <ViewDocumentDetailsModal
          token={token}
          documentId={viewingDocId}
          onClose={() => setViewingDocId(null)}
        />
      )}
    </div>
  );
}
