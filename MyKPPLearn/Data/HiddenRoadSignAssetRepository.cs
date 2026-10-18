using System;
using System.Data;
using System.Data.SqlClient;

namespace MyKPPLearn.Data
{
    /// <summary>
    /// Data access for Road Sign Assets: the isolated road-sign images (for example "Pedestrian
    /// Crossing") that Hidden Road Sign Game hotspots refer to. This is NOT the large Scene Image
    /// of a Game Set; those live on HiddenRoadSignPractices.
    /// </summary>
    public static class HiddenRoadSignAssetRepository
    {
        private const int SqlForeignKeyViolation = 547;

        /// <summary>All assets with UsageCount (number of Game hotspots that use the asset).</summary>
        public static DataTable GetAllForAdmin()
        {
            const string sql = @"
                SELECT a.RoadSignAssetId, a.Name, a.ImagePath, a.IsActive,
                       (SELECT COUNT(*) FROM HiddenRoadSignItems i WHERE i.RoadSignAssetId = a.RoadSignAssetId) AS UsageCount
                FROM HiddenRoadSignAssets a
                ORDER BY a.Name ASC, a.RoadSignAssetId ASC";

            return DatabaseHelper.ExecuteQuery(sql);
        }

        public static DataRow GetById(int assetId)
        {
            const string sql = @"
                SELECT a.RoadSignAssetId, a.Name, a.ImagePath, a.IsActive,
                       (SELECT COUNT(*) FROM HiddenRoadSignItems i WHERE i.RoadSignAssetId = a.RoadSignAssetId) AS UsageCount
                FROM HiddenRoadSignAssets a
                WHERE a.RoadSignAssetId = @AssetId";

            DataTable table = DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@AssetId", assetId));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        /// <summary>
        /// Assets that may be chosen in the hotspot editor of a Game Set: every Active asset, plus
        /// any Inactive asset that is already used by a hotspot of that same Set (so an existing,
        /// already-linked hotspot stays editable). Inactive assets are never offered for NEW assignments.
        /// </summary>
        public static DataTable GetSelectableForSet(int practiceId)
        {
            const string sql = @"
                SELECT a.RoadSignAssetId, a.Name, a.ImagePath, a.IsActive
                FROM HiddenRoadSignAssets a
                WHERE a.IsActive = 1
                   OR EXISTS (SELECT 1 FROM HiddenRoadSignItems i
                              WHERE i.RoadSignAssetId = a.RoadSignAssetId AND i.HiddenRoadSignPracticeId = @PracticeId)
                ORDER BY a.Name ASC, a.RoadSignAssetId ASC";

            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@PracticeId", practiceId));
        }

        /// <summary>True when another road sign (not <paramref name="excludeAssetId"/>) already has this name, ignoring case.</summary>
        public static bool NameExists(string name, int excludeAssetId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM HiddenRoadSignAssets WHERE LOWER(LTRIM(RTRIM(Name))) = LOWER(@Name) AND RoadSignAssetId <> @ExcludeId",
                new SqlParameter("@Name", (name ?? string.Empty).Trim()),
                new SqlParameter("@ExcludeId", excludeAssetId));

            return Convert.ToInt32(result) > 0;
        }

        public static int Create(string name, string imagePath, bool isActive)
        {
            const string sql = @"
                INSERT INTO HiddenRoadSignAssets (Name, ImagePath, IsActive, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.RoadSignAssetId
                VALUES (@Name, @ImagePath, @IsActive, SYSUTCDATETIME(), SYSUTCDATETIME());";

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql,
                new SqlParameter("@Name", name),
                new SqlParameter("@ImagePath", imagePath),
                new SqlParameter("@IsActive", isActive)));
        }

        /// <param name="imagePath">New image path, or null to keep the existing image unchanged.</param>
        public static void Update(int assetId, string name, string imagePath, bool isActive)
        {
            const string sql = @"
                UPDATE HiddenRoadSignAssets
                SET Name = @Name, ImagePath = ISNULL(@ImagePath, ImagePath), IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
                WHERE RoadSignAssetId = @AssetId";

            DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@AssetId", assetId),
                new SqlParameter("@Name", name),
                new SqlParameter("@ImagePath", (object)imagePath ?? DBNull.Value),
                new SqlParameter("@IsActive", isActive));
        }

        public static void SetActive(int assetId, bool isActive)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE HiddenRoadSignAssets SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE RoadSignAssetId = @AssetId",
                new SqlParameter("@AssetId", assetId),
                new SqlParameter("@IsActive", isActive));
        }

        /// <summary>
        /// Deletes the asset only if no Game hotspot uses it (the guard and the delete are one
        /// statement, and the foreign key is a second safety net). Returns true if it was deleted.
        /// </summary>
        public static bool DeleteIfUnused(int assetId)
        {
            const string sql = @"
                DELETE FROM HiddenRoadSignAssets
                WHERE RoadSignAssetId = @AssetId
                  AND NOT EXISTS (SELECT 1 FROM HiddenRoadSignItems WHERE RoadSignAssetId = @AssetId)";

            try
            {
                return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter("@AssetId", assetId)) > 0;
            }
            catch (SqlException ex) when (ex.Number == SqlForeignKeyViolation)
            {
                return false;
            }
        }
    }
}
