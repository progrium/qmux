
export const OpenID = 100;
export const OpenConfirmID = 101;
export const OpenFailureID = 102;
export const WindowAdjustID = 103;
export const DataID = 104;
export const EofID = 105;
export const CloseID = 106;

export var payloadSizes = new Map([
	[OpenID,         12],
	[OpenConfirmID,  16],
	[OpenFailureID,  4],
	[WindowAdjustID, 8],
	[DataID,         8],
	[EofID,          4],
	[CloseID,        4],
]);

export interface Message {
	ID: number;
}

export interface OpenMessage {
	ID: 			100;
	senderID:       number;
	windowSize:   	number;
	maxPacketSize: 	number;
}

export interface OpenConfirmMessage {
	ID: 			101;
	channelID: 		number;
	senderID: 		number;
	windowSize: 	number;
	maxPacketSize: 	number;
}

export interface OpenFailureMessage {
	ID: 		102;
	channelID: 	number;
}

export interface WindowAdjustMessage {
	ID: 				103;
	channelID: 			number;
	additionalBytes: 	number;
}

export interface DataMessage {
	ID: 		104;
	channelID: 	number;
	length: 	number;
	data: 		Uint8Array;
}

export interface EOFMessage {
	ID: 		105;
	channelID: 	number;
}

export interface CloseMessage {
	ID: 		106;
	channelID: 	number;
}

export type ChannelMessage = (
	OpenConfirmMessage | 
	OpenFailureMessage |
	WindowAdjustMessage |
	DataMessage |
	EOFMessage |
	CloseMessage);

export type AnyMessage = ChannelMessage | OpenMessage;