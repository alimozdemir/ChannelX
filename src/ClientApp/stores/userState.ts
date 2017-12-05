import { ActionContext, Store } from "vuex";
import { getStoreAccessors } from "vuex-typescript";
import { State as RootState } from "./store";

export interface UserState {
    userId : string,
    authKey : string,
    userName: string
}

type UserContext = ActionContext<UserState, RootState>;


export const userController = {
    namespaced: true,
    state: {
        userId: "",
        authKey: "",
        userName: ""
    },

    getters: {
        getUserId(state: UserState) {
            return state.userId;
        },
        getAuthKey(state: UserState){
            return state.authKey;
        },
        getUserName(state: UserState){
            return state.userName;
        }
    },

    mutations: {
        setUserId(state: UserState, value: string) {
            state.userId = value;
        },

        setUserName(state: UserState, value: string) {
            state.userName = value;
        },
        setAuthKey(state: UserState, value: string){
            state.authKey = value;
            if(value === "")
                localStorage.removeItem('auth');
            else
                localStorage.setItem('auth', value);
        }
    },

    actions: {
        dispatchAuthKey (context : UserContext){
            let val = localStorage.getItem('auth');
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
    readUserName: read(getters.getUserName),
    dispatchAuthKey : dispatch(actions.dispatchAuthKey),
    commitAuthKey: commit(mutations.setAuthKey),
    commitUserId: commit(mutations.setUserId),
    commitUserName: commit(mutations.setUserName)
}