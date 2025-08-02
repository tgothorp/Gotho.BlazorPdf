export class PdfMetadata {
    public author: string;
    public creator: string;
    public keywords: string;
    public producer: string;
    public subject: string;
    public title: string;
    public formatVersion: string;
    public creationDate: Date;
    public modifiedDate: Date;
    public customMetadata: Record<string, string>;

    constructor(author: string, 
                creator: string,
                keywords: string,
                producer: string,
                subject: string,
                title: string,
                formatVersion: string, 
                creationDate: Date, 
                modifiedDate: Date,
                customMetadata: Record<string, string> = {}) {
        this.author = author;
        this.creator = creator;
        this.subject = subject;
        this.keywords = keywords;
        this.producer = producer;
        this.title = title;
        this.formatVersion = formatVersion;
        this.creationDate = creationDate;
        this.modifiedDate = modifiedDate;
        this.customMetadata = customMetadata;
    }
}