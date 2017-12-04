import { ActionContext, Store } from "vuex";
import { getStoreAccessors } from "vuex-typescript";
import { State as RootState } from "./store";

export interface UserState {
    userId : string,
    authKey : string
}

type UserContext = ActionContext<UserState, RootState>;


export const userController = {
    namespaced: true,
    state: {
        userId: "",
        authKey: ""
    },

    getters: {
        getUserId(state: UserState) {
            return state.userId;
        },
        getAuthKey(state: UserState){
            return state.authKey;
        }
    },

    mutations: {
        setUserId(state: UserState, value: string) {
            state.userId = value;
        },
        setAuthKey(state: UserState, value: string){
            state.authKey = value;
            console.log("[setAuthKey]", value)
            if(value === "")
                localStorage.removeItem('auth');
            else
                localStorage.setItem('auth', value);
        }
    },

    actions: {
        dispatchAuthKey (context : UserContext){
            let val = localStorage.getItem('auth');
            console.log("[dispatchAuthKey]", val)
            if(val == undefined)
                val = '';
            commitAuthKey(context, val);
        }
    },
};

const { commit, read, dispatch } =
     getStoreAccessors<UserState, RootState>("userController");

const getters = userController.getters;
const actions = userController.actions;
const mutations = userController.mutations;

export const commitAuthKey = commit(mutations.setAuthKey);

export const UserStore = {
    readUserId : read(getters.getUserId),
    readAuthKey : read(getters.getAuthKey),
    dispatchAuthKey : dispatch(actions.dispatchAuthKey),
    commitAuthKey: commit(mutations.setAuthKey),
    commitUserId: commit(mutations.setUserId)
}