// Source: https://david-homer.blogspot.com/2016/08/document-windows-advanced-audit-policy.html

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:exe /out:auditpol.exe auditpol.cs


using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Provides management functions of the advanced audit policy (audit policy subcategory settings).
/// </summary>
public class AdvancedAuditPolicyWrapper
{
    private static void PrintUsage()
    {
        Console.WriteLine(@"Enumerates the audit policy configuration of the current host. Requires administrative privileges.
    
USAGE:
    auditpol.exe [/?]");
    }

    public static void Main(string[] args)
    {
        try
        {
            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToUpper())
                {
                    case "/?":
                        PrintUsage();
                        return;
                }
            }

            string auditInfo;

            Console.WriteLine("System audit policy");
            Console.WriteLine("Category/Subcategory                      Setting");

            foreach (string category_guid_str in GetCategoryIdentifiers())
            {
                Console.WriteLine(GetCategoryDisplayName(category_guid_str));

                foreach (string subcat_guid_str in GetSubCategoryIdentifiers(category_guid_str))
                {
                    Console.Write("    " + String.Format("{0,-40}", GetSubCategoryDisplayName(subcat_guid_str)));

                    try
                    {
                        auditInfo = GetSystemPolicy(subcat_guid_str).AuditingInformation.ToString();

                        // Post-process to make the output match the native tool
                        if (auditInfo == "None")
                        {
                            auditInfo = "No Auditing";
                        }

                        auditInfo = auditInfo.Replace(", ", " and ");

                        Console.WriteLine(auditInfo);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message.Trim());
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message.Trim());
        }
        finally
        {
            Console.WriteLine("\nDONE");
        }
    }

    /// <summary>
    /// Initializes a new instance of the CENTREL.XIA.Management.AdvancedAuditPolicyWrapper class.
    /// </summary>
    public AdvancedAuditPolicyWrapper()
    {

    }


    /// <summary>
    /// The AuditEnumerateCategories function enumerates the available audit-policy categories.
    /// </summary>
    /// <param name="ppAuditCategoriesArray">A pointer to a single buffer that contains both an array of pointers to GUID structures and the structures themselves. </param>
    /// <param name="pCountReturned">A pointer to the number of elements in the ppAuditCategoriesArray array.</param>
    /// <returns>A System.Boolean value that indicates whether the function completed successfully.</returns>
    /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/aa375636(v=vs.85).aspx</remarks>
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AuditEnumerateCategories(out IntPtr ppAuditCategoriesArray, out uint pCountReturned);


    /// <summary>
    /// The AuditLookupCategoryName function retrieves the display name of the specified audit-policy category.
    /// </summary>
    /// <param name="pAuditCategoryGuid">A pointer to a GUID structure that specifies an audit-policy category.</param>
    /// <param name="ppszCategoryName">The address of a pointer to a null-terminated string that contains the display name of the audit-policy category specified by the pAuditCategoryGuid function.</param>
    /// <returns>A System.Boolean value that indicates whether the function completed successfully.</returns>
    /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/aa375687(v=vs.85).aspx</remarks>
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AuditLookupCategoryName(ref Guid pAuditCategoryGuid, out StringBuilder ppszCategoryName);


    /// <summary>
    /// The AuditEnumerateSubCategories function enumerates the available audit-policy subcategories.
    /// </summary>
    /// <param name="pAuditCategoryGuid">The GUID of an audit-policy category for which subcategories are enumerated. If the value of the bRetrieveAllSubCategories parameter is TRUE, this parameter is ignored.</param>
    /// <param name="bRetrieveAllSubCategories">TRUE to enumerate all audit-policy subcategories; FALSE to enumerate only the subcategories of the audit-policy category specified by the pAuditCategoryGuid parameter.</param>
    /// <param name="ppAuditSubCategoriesArray">A pointer to a single buffer that contains both an array of pointers to GUID structures and the structures themselves. The GUID structures specify the audit-policy subcategories available on the computer.</param>
    /// <param name="pCountReturned">A pointer to the number of audit-policy subcategories returned in the ppAuditSubCategoriesArray array.</param>
    /// <returns>A System.Boolean value that indicates whether the function completed successfully.</returns>
    /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/aa375648(v=vs.85).aspx</remarks>
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AuditEnumerateSubCategories(ref Guid pAuditCategoryGuid, bool bRetrieveAllSubCategories, out IntPtr ppAuditSubCategoriesArray, out uint pCountReturned);


    /// <summarThe AuditLookupSubCategoryName function retrieves the display name of the specified audit-policy subcategory. y>
    /// The AuditLookupSubCategoryName function retrieves the display name of the specified audit-policy subcategory.
    /// </summary>
    /// <param name="pAuditSubCategoryGuid">A pointer to a GUID structure that specifies an audit-policy subcategory.</param>
    /// <param name="ppszSubCategoryName">The address of a pointer to a null-terminated string that contains the display name of the audit-policy subcategory specified by the pAuditSubCategoryGuid parameter.</param>
    /// <returns>A System.Boolean value that indicates whether the function completed successfully.</returns>
    /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/aa375693(v=vs.85).aspx</remarks>
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AuditLookupSubCategoryName(ref Guid pAuditSubCategoryGuid, out StringBuilder ppszSubCategoryName);


    /// <summary>
    /// The AuditFree function frees the memory allocated by audit functions for the specified buffer.
    /// </summary>
    /// <param name="buffer">A pointer to the buffer to free.</param>
    /// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/aa375654(v=vs.85).aspx</remarks>
    [DllImport("advapi32.dll")]
    private static extern void AuditFree(IntPtr buffer);


    /// <summary>
    /// The AuditQuerySystemPolicy function retrieves system audit policy for one or more audit-policy subcategories.
    /// </summary>
    /// <param name="pSubCategoryGuids">A pointer to an array of GUID values that specify the subcategories for which to query audit policy. </param>
    /// <param name="PolicyCount">The number of elements in each of the pSubCategoryGuids and ppAuditPolicy arrays.</param>
    /// <param name="ppAuditPolicy">A pointer to a single buffer that contains both an array of pointers to AUDIT_POLICY_INFORMATION structures and the structures themselves. </param>
    /// <returns>https://msdn.microsoft.com/en-us/library/windows/desktop/aa375702(v=vs.85).aspx</returns>
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AuditQuerySystemPolicy(Guid pSubCategoryGuids, uint PolicyCount, out IntPtr ppAuditPolicy);


    /// <summary>
    /// Gets the GUIDs of the audit categories.
    /// </summary>
    /// <returns>The GUIDs of the audit categories on the local machine.</returns>
    private static StringCollection GetCategoryIdentifiers()
    {
        StringCollection identifiers = new StringCollection();
        IntPtr buffer;
        uint categoryCount;
        bool success = AuditEnumerateCategories(out buffer, out categoryCount);
        if (!success) { throw new Win32Exception(Marshal.GetLastWin32Error()); }
        for (int i = 0, elemOffs = (int)buffer; i < categoryCount; i++)
        {
            Guid guid = (Guid)Marshal.PtrToStructure((IntPtr)elemOffs, typeof(Guid));
            identifiers.Add(Convert.ToString(guid));
            elemOffs += Marshal.SizeOf(typeof(Guid));
        }
        AuditFree(buffer);
        return identifiers;
    }


    /// <summary>
    /// Returns the display name of the audit category with the specified GUID.
    /// </summary>
    /// <param name="guid">The GUID of the category for which the display name should be returned.</param>
    /// <returns>The display name of the category - for example "Account Management".</returns>
    private static String GetCategoryDisplayName(String guid)
    {
        return GetCategoryDisplayName(new Guid(guid));
    }


    /// <summary>
    /// Returns the display name of the audit category with the specified GUID.
    /// </summary>
    /// <param name="guid">The GUID of the category for which the display name should be returned.</param>
    /// <returns>The display name of the category - for example "Account Management".</returns>
    private static String GetCategoryDisplayName(Guid guid)
    {
        StringBuilder buffer = new StringBuilder();
        bool success = AuditLookupCategoryName(ref guid, out buffer);
        if (!success) { throw new Win32Exception(Marshal.GetLastWin32Error()); }
        if (buffer == null)
        {
            throw new ArgumentException(String.Format("Category Display Name Not Found for {0}", guid));
        }
        String categoryDisplayName = buffer.ToString();
        buffer = null;
        return categoryDisplayName;
    }


    /// <summary>
    /// Gets the GUIDs of the audit subcategories of the specified category.
    /// </summary>
    /// <param name="guid">The GUID of the category for which the subcategories should be returned.</param>
    /// <returns>The GUIDs of the audit subcategories for the specified category.</returns>
    private static StringCollection GetSubCategoryIdentifiers(String categoryGuid)
    {
        return GetSubCategoryIdentifiers(new Guid(categoryGuid));
    }


    /// <summary>
    /// Gets the GUIDs of the audit subcategories of the specified category.
    /// </summary>
    /// <param name="guid">The GUID of the category for which the subcategories should be returned.</param>
    /// <returns>The GUIDs of the audit subcategories for the specified category.</returns>
    private static StringCollection GetSubCategoryIdentifiers(Guid categoryGuid)
    {
        StringCollection identifiers = new StringCollection();
        IntPtr buffer;
        uint subCategoryCount;
        bool success = AuditEnumerateSubCategories(ref categoryGuid, false, out buffer, out subCategoryCount);
        if (!success) { throw new Win32Exception(Marshal.GetLastWin32Error()); }
        for (int i = 0, elemOffs = (int)buffer; i < subCategoryCount; i++)
        {
            Guid guid = (Guid)Marshal.PtrToStructure((IntPtr)elemOffs, typeof(Guid));
            identifiers.Add(Convert.ToString(guid));
            elemOffs += Marshal.SizeOf(typeof(Guid));
        }
        AuditFree(buffer);
        return identifiers;
    }


    /// <summary>
    /// Returns the display name of the audit subcategory with the specified GUID.
    /// </summary>
    /// <param name="guid">The GUID of the subcategory for which the display name should be returned.</param>
    /// <returns>The display name of the subcategory - for example "Audit Credential Validation".</returns>
    private static String GetSubCategoryDisplayName(String guid)
    {
        return GetSubCategoryDisplayName(new Guid(guid));
    }


    /// <summary>
    /// Returns the display name of the audit subcategory with the specified GUID.
    /// </summary>
    /// <param name="guid">The GUID of the subcategory for which the display name should be returned.</param>
    /// <returns>The display name of the subcategory - for example "Audit Credential Validation".</returns>
    private static String GetSubCategoryDisplayName(Guid guid)
    {
        StringBuilder buffer = new StringBuilder();
        bool success = AuditLookupSubCategoryName(ref guid, out buffer);
        if (!success) { throw new Win32Exception(Marshal.GetLastWin32Error()); }
        String subCategoryDisplayName = buffer.ToString();
        buffer = null;
        return subCategoryDisplayName;
    }


    /// <summary>
    /// Gets the audit policy configured for the specified subcategory GUID.
    /// </summary>
    /// <param name="subCategoryGuid">The GUID of the subcategory for which the policy should be returned.</param>
    /// <returns>Returns an AUDIT_POLICY_INFORMATION that contains information about the policy.</returns>
    private static AUDIT_POLICY_INFORMATION GetSystemPolicy(String subCategoryGuid)
    {
        return GetSystemPolicy(new Guid(subCategoryGuid));
    }


    /// <summary>
    /// Gets the audit policy configured for the specified subcategory GUID.
    /// </summary>
    /// <param name="subCategoryGuid">The GUID of the subcategory for which the policy should be returned.</param>
    /// <returns>Returns an AUDIT_POLICY_INFORMATION that contains information about the policy.</returns>
    private static AUDIT_POLICY_INFORMATION GetSystemPolicy(Guid subCategoryGuid)
    {
        StringCollection identifiers = new StringCollection();
        IntPtr buffer;
        bool success = AuditQuerySystemPolicy(subCategoryGuid, 1, out buffer);
        if (!success) { throw new Win32Exception(Marshal.GetLastWin32Error()); }
        AUDIT_POLICY_INFORMATION policyInformation = new AUDIT_POLICY_INFORMATION();
        try
        {
            policyInformation = (AUDIT_POLICY_INFORMATION)Marshal.PtrToStructure(buffer, typeof(AUDIT_POLICY_INFORMATION));
            AuditFree(buffer);
        }
        catch
        {
            throw new Exception("ERROR 5: Insufficient privileges");
        }

        return policyInformation;
    }
}



/// <summary>
/// The AUDIT_POLICY_INFORMATION structure specifies a security event type and when to audit that type.
/// </summary>
/// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/aa965467(v=vs.85).aspx</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct AUDIT_POLICY_INFORMATION
{

    /// <summary>
    /// A GUID structure that specifies an audit subcategory.
    /// </summary>
    public Guid AuditSubCategoryGuid;

    /// <summary>
    /// A set of bit flags that specify the conditions under which the security event type specified by the AuditSubCategoryGuid and AuditCategoryGuid members are audited.
    /// </summary>
    public AUDIT_POLICY_INFORMATION_TYPE AuditingInformation;

    /// <summary>
    /// A GUID structure that specifies an audit-policy category.
    /// </summary>
    public Guid AuditCategoryGuid;

}



/// <summary>
/// Represents the auditing type.
/// </summary>
[Flags]
public enum AUDIT_POLICY_INFORMATION_TYPE
{
    /// <summary>
    /// Do not audit the specified event type.
    /// </summary>
    None = 0,

    /// <summary>
    /// Audit successful occurrences of the specified event type.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Audit failed attempts to cause the specified event type.
    /// </summary>
    Failure = 2,
}