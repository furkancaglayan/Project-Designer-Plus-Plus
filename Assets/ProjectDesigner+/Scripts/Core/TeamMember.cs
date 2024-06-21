using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A class to define team members in the project.
    /// </summary>
    [Serializable]
    public class TeamMember
    {
        private const string ProfilePicture = "profile";
        /// <summary>
        /// Full name of the team member.
        /// </summary>
        [SerializeField]
        public string FullName;
        /// <summary>
        /// Role of the team member.
        /// </summary>
        [SerializeField]
        public string Role;
        /// <summary>
        /// Image of the team member.
        /// </summary>
        [SerializeField]
        public Texture2D Image;

        public TeamMember()
        {

        }

        public TeamMember(string fullName, string role, Texture2D image)
        {
            FullName = fullName;
            Role = role;
            Image = image;
        }

        private string GetFullName()
        {
            string s = FullName;
            if (string.IsNullOrEmpty(s.Trim()))
            {
                return "Team Member";
            }

            return s;
        }

        /// <summary>
        /// Returns the display name.
        /// </summary>
        /// <returns></returns>
        public string GetDisplayName()
        {
            return $"{GetFullName()} ({Role})";
        }

        /// <summary>
        /// Returns the member image. If it doesn't exist, this returns a default profile image.
        /// </summary>
        /// <returns></returns>
        public Texture2D GetMemberImage()
        {
            if (Image == null)
            {
                return GUIStyleCollection.GetTexture(ProfilePicture);
            }

            return Image;
        }

        public override string ToString()
        {
            return GetFullName();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            TeamMember otherMember = (TeamMember)obj;

            // Check if all properties are equal
            return FullName == otherMember.FullName &&
                   Role == otherMember.Role &&
                   Texture2DEqualityEquals(Image, otherMember.Image);
        }

        public override int GetHashCode()
        {
            // Combine hash codes of properties to create a unique hash code for the object
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (FullName != null ? FullName.GetHashCode() : 0);
                hash = hash * 23 + (Role != null ? Role.GetHashCode() : 0);
                hash = hash * 23 + (Image != null ? Image.GetHashCode() : 0);
                return hash;
            }
        }

        // Helper method to compare Texture2D instances
        private static bool Texture2DEqualityEquals(Texture2D tex1, Texture2D tex2)
        {
            if (tex1 == tex2)
            {
                return true; // Same reference or both null
            }

            if (tex1 == null || tex2 == null)
            {
                return false; // One is null while the other is not
            }

            if (tex1.width != tex2.width || tex1.height != tex2.height)
            {
                return false; // Different dimensions
            }

            Color[] pixels1 = tex1.GetPixels();
            Color[] pixels2 = tex2.GetPixels();

            for (int i = 0; i < pixels1.Length; i++)
            {
                if (pixels1[i] != pixels2[i])
                {
                    return false; // Pixels are different
                }
            }

            return true; // All checks passed, textures are equal
        }
    }
}
