using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data {
    public class Sorting {
        public enum By {
            Money = 0,
            Exp = 1
        };

        public static Profile[] GetHumanityTop(By by) {

            Profile[] profilesArr = new Profile[Vars.Humanity.Count];
            Vars.Humanity.Values.CopyTo(profilesArr, 0);

            Profile temp;
            if(by == By.Money) {
                for(int i = 0; i < profilesArr.Length - 1; i++) {

                    for(int j = i + 1; j < profilesArr.Length; j++) {

                        if(profilesArr[i].Balance.Money < profilesArr[j].Balance.Money) {
                            temp = profilesArr[i];
                            profilesArr[i] = profilesArr[j];
                            profilesArr[j] = temp;
                        }
                    }
                }
            }
            if(by == By.Exp) {
                for(int i = 0; i < profilesArr.Length - 1; i++) {

                    for(int j = i + 1; j < profilesArr.Length; j++) {

                        if(profilesArr[i].Level.TotalExperience < profilesArr[j].Level.TotalExperience) {
                            temp = profilesArr[i];
                            profilesArr[i] = profilesArr[j];
                            profilesArr[j] = temp;
                        }
                    }
                }
            }

            return profilesArr;     
        }

    }
}
